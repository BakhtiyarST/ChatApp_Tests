using ChatAppDB_Test.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatAppDB_Test.Models;

namespace ChatAppDB_Test;

public class Server
{
	private static bool flagContinue = true;
	private static CancellationTokenSource cts = new CancellationTokenSource();
	private static CancellationToken ct = cts.Token;
	private Dictionary<String, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();
	private UdpClient? udpClient;


	// Server methods
	public void RequestUnreadMessages(MessageUDP msg, IPEndPoint endPoint)
	{
		int id = 0, count = 1;
		string unreadMessages = string.Empty;
		using (var ctx = new Context())
		{
			var user = ctx.Users.FirstOrDefault(x => x.Name == msg.FromName);
			var messages = ctx.Messages.Where(x => x.FromUserId == user.Id && x.Received == false).ToList();
			if (messages.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Unread messages:\n");
				foreach (var message in messages)
				{
					stringBuilder.Append("\n");
					stringBuilder.Append("Message ");
					stringBuilder.Append(count++.ToString());
					stringBuilder.Append("\n");
					stringBuilder.Append(message.Text);
					stringBuilder.Append("\n");
				}
				unreadMessages = stringBuilder.ToString();
				// Отправляем подготовленный список клиенту
				var msgJson = new MessageUDP()
				{
					Id = id,
					Command = Command.Message,
					ToName = msg.FromName,
					FromName = "Server",
					Text = unreadMessages
				}.ToJson();
				byte[] msgBytes = Encoding.ASCII.GetBytes(msgJson);
				// Отправляем сообщение клиенту
				udpClient.Send(msgBytes, msgBytes.Length, endPoint);
				Console.WriteLine($"Message sent, from = Server to = {msg.FromName}");
			}
		}
	}

	public void RelayMessage(MessageUDP message)
	{
		int id = 0;
		if (clients.TryGetValue(message.ToName, out IPEndPoint ep))
		{
			// Добавляем сообщение в базу данных
			using (var ctx = new Context())
			{
				var fromUser = ctx.Users.First(x => x.Name == message.FromName);
				var toUser = ctx.Users.First(x => x.Name == message.ToName);
				var msg = new ChatAppDB_Test.Models.Message
				{
					FromUser = fromUser,
					ToUser = toUser,
					Received = false,
					Text = message.Text
				};
				ctx.Messages.Add(msg);
				ctx.SaveChanges();
				id = msg.Id;
			}
			// Подготавливаем сообщение для пересылки
			var forwardMessageJson = new MessageUDP()
			{
				Id = id,
				Command = Command.Message,
				ToName = message.ToName,
				FromName = message.FromName,
				Text = message.Text
			}.ToJson();
			byte[] forwardBytes = Encoding.ASCII.GetBytes(forwardMessageJson);
			// Отправляем сообщение клиенту
			udpClient.Send(forwardBytes, forwardBytes.Length, ep);
			Console.WriteLine($"Message Relayed, from = {message.FromName} to = {message.ToName}");
		}
		else
		{
			Console.WriteLine("Client is not found.");
		}
	}

	public void ConfirmMessageReceived(int? id)
	{
		Console.WriteLine("Message confirmation id=" + id);
		using (var ctx = new Context())
		{
			var msg = ctx.Messages.FirstOrDefault(x => x.Id == id);
			if (msg != null)
			{
				msg.Received = true;
				ctx.SaveChanges();
			}
		}
	}

	public void Register(MessageUDP msg, IPEndPoint ep)
	{
		Console.WriteLine("Registering message from: " + msg.FromName);
		clients.Add(msg.FromName, ep);
		using (var ctx = new Context())
		{
			if (ctx.Users.FirstOrDefault(x => x.Name == msg.FromName) != null) return;
			ctx.Add(new User { Name = msg.FromName });
			ctx.SaveChanges();
		}
	}

	public void ProcessMessage(MessageUDP msg, IPEndPoint endPoint)
	{
		Console.WriteLine($"Received message from {msg.FromName} for {msg.ToName} with command {msg.Command}");
		Console.WriteLine(msg.Text);

		if (msg.Command == Command.Register)
		{
			Register(msg, new IPEndPoint(endPoint.Address, endPoint.Port));
		}
		else if (msg.Command == Command.Confirmation)
		{
			Console.WriteLine("Confirmation received");
			ConfirmMessageReceived(msg.Id);
		}
		else if (msg.Command == Command.Message)
		{
			RelayMessage(msg);
		}
		else if (msg.Command == Command.List)
		{
			RequestUnreadMessages(msg, endPoint);
		}
		else
		{
			Console.WriteLine("Command is not correct.");
		}
	}

	public void Operate()
	{
		Console.WriteLine("The server is ready for accepting messages.");
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
		UdpClient udpClient = new UdpClient(12345);

		while (true)
		{
			byte[] buffer = udpClient.Receive(ref endPoint);
			string data = Encoding.UTF8.GetString(buffer);

			Console.Out.WriteLineAsync(data);
			try
			{
				var msg = MessageUDP.FromJson(data);
				ProcessMessage(msg, endPoint);
			}
			catch (Exception ex)
			{
				Console.Out.WriteLineAsync("Ошибка при обработке сообщения: " + ex.Message);
			}
		}
	}
}

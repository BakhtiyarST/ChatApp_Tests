using ChatAppDB_Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using ChatAppDB_Test;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatAppDB_Test;

public class Client
{
	private int cmd = 0;
	private Command command;
	private string? cmdStr;
	private string? fromName;
	private string? toName = "";
	private string? msgText;
	private IPEndPoint endPoint;
	private UdpClient client;

	public Client(string name)
	{
		fromName = name;
	}

	bool PickCommandFromKeyPad()
	{
		Console.WriteLine("Enter the digit, corresponding to a command (1 - Register, 2 - Message, 3 - Confirmation, 4 - List):");
		cmdStr = Console.ReadLine();
		cmd = int.Parse(cmdStr);
		if ((cmd == 1) || (cmd == 2) || (cmd == 3) || (cmd == 4))
			return true;
		else
		{
			Console.WriteLine("Command is unknown");
			return false;
		}
	}

	bool PickDestinationFromKeyPad()
	{
		Console.WriteLine("Enter the destination person name:");
		toName = Console.ReadLine();
		if (!toName.IsNullOrEmpty())
			return true;
		else
		{
			Console.WriteLine("The name should not be empty. Restarting the information collection.");
			return false;
		}
	}

	public void Operate()
	{
		string msgJson, msgJsonR;
		byte[] bytes, bytesR;
		MessageUDP msg, msgR;

		Console.WriteLine("The client is up and ready.");
		endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
		client = new UdpClient();
		while (true)
		{
			if (PickCommandFromKeyPad())
			{
				if (PickDestinationFromKeyPad())
				{
					Console.WriteLine("Enter the message:");
					msgText = Console.ReadLine();
				}
				else
					continue;
			}
			else
				continue;

			switch (cmd)
			{
				case 1: command = Command.Register; break;
				case 2: command = Command.Message; break;
				case 3: command = Command.Confirmation; break;
				case 4: command = Command.List; break;
				default: { } break;
			}

			msg = new MessageUDP(fromName, this.toName, command, msgText);
			msgJson = msg.ToJson();
			bytes = Encoding.UTF8.GetBytes(msgJson);
			client.Send(bytes, bytes.Length, endPoint);
			bytesR = client.Receive(ref endPoint);
			msgJsonR = Encoding.UTF8.GetString(bytesR);
			msgR = MessageUDP.FromJson(msgJsonR);
			Console.WriteLine(msgR.ToString());
		}
	}
}

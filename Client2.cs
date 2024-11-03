using ChatAppDB_Test.Abstraction;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppDB_Test;

internal class Client2 : IMessageSource
{
	private readonly string _name;
	private readonly IMessageSource _messageSource;
	private readonly IPEndPoint _peerEndpoint;

	public Client2(MessageSource messageSource, IPEndPoint peerEndPoint, string name)
	{
		_messageSource = messageSource;
		_peerEndpoint = peerEndPoint;
		_name = name;
	}

	private void Regestered()
	{
		var messageJson = new MessageUDP()
		{
			Command = Command.Register,
			FromName = _name
		};
		_messageSource.SendMessage(messageJson, _peerEndpoint);
	}

	public void ClientSender()
	{
		Regestered();
		while(true)
		{
			Console.WriteLine("Enter the message:");
			string text =Console.ReadLine();
			Console.WriteLine("Enter the name:");
			string name = Console.ReadLine();
			if (string.IsNullOrEmpty(name))
				continue;
			MessageUDP messageJson = new MessageUDP()
			{
				Text = text,
				FromName = _name,
				ToName = name
			};
			_messageSource.SendMessage(messageJson, _peerEndpoint);
		}
	}

	public void ClientListener()
	{
		IPEndPoint ep = new IPEndPoint(_peerEndpoint.Address, _peerEndpoint.Port);

		Regestered();
		
		while(true)
		{
			MessageUDP message = _messageSource.ReceiveMessage(ref ep);
			Console.WriteLine(message.ToString());
		}
	}

	public void SendMessage(MessageUDP message, IPEndPoint endPoint)
	{
		_messageSource.SendMessage(message, endPoint);
	}

	public MessageUDP ReceiveMessage(ref IPEndPoint endPoint)
	{
		return _messageSource.ReceiveMessage(ref endPoint);
	}
}

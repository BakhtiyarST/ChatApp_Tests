using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatAppDB_Test;

public enum Command
{
	Register,
	Message,
	Confirmation,
	List
}
public class MessageUDP
{

	public MessageUDP()
	{
		Id = -1;
		Command = Command.Register;
		FromName = String.Empty;
		ToName = String.Empty;
		Text = String.Empty;
	}

	public MessageUDP(string fromName, string toName, Command cmd, string text)
	{
		FromName = fromName;
		ToName = toName;
		Command = cmd;
		Text = text;
		// this.Date = DateTime.Now;
	}
	public Command Command { get; set; }
	public int? Id { get; set; }
	public string FromName { get; set; }
	public string ToName { get; set; }
	public string Text { get; set; }
	
	/*
	public override string ToString()
	{
		// return $"{Text}\n from: {FromName}";
		return $"{DateTime.Now}\n Message received: {Text}\n from: {FromName}";
	}
	*/

	public string ToJson()
	{
		return JsonSerializer.Serialize(this);
	}
	
	public static MessageUDP FromJson(string json)
	{
		return JsonSerializer.Deserialize<MessageUDP>(json);
	}
}

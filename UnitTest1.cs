namespace TestsForChatApp;
using ChatAppDB_Test;
using ChatAppDB_Test.Abstraction;
using System.Net;

// using ChatAppDB_Test.Abstraction;

public class Tests
{
	IMessageSource _source;
	IPEndPoint _endPoint;

	[SetUp]
	public void Setup()
	{
		// _endPoint = new IPEndPoint(IPAddress.Any, 0);
		_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
	}

	[Test]
	public void TestReceiveMessage()
	{
		// Assert.Pass();
		_source=new MockMessageSource();
		var result=_source.ReceiveMessage(ref _endPoint);

		Assert.IsNotNull(result);
		Assert.IsNotNull(result.FromName);
		Assert.That("Serik",Is.EqualTo(result.FromName));
		Assert.That(Command.Register, Is.EqualTo(result.Command));
	}
}

public class MockMessageSource : IMessageSource
{

	private Queue<MessageUDP> messages = new Queue<MessageUDP>();

	public MockMessageSource()
	{
		messages.Enqueue(new MessageUDP { Command = Command.Register, FromName="Serik" });
		messages.Enqueue(new MessageUDP { Command = Command.Register, FromName = "Aigul" });
		messages.Enqueue(new MessageUDP { Command = Command.Register, FromName = "Serik", ToName = "Aigul", Text="Hi from Serik" });
		messages.Enqueue(new MessageUDP { Command = Command.Register, FromName = "Aigul", ToName = "Serik", Text ="Hello from Aigul" });
	}

	public MessageUDP ReceiveMessage(ref IPEndPoint endPoint)
	{
		return messages.Peek();
	}

	public void SendMessage(MessageUDP message, IPEndPoint endPoint)
	{
		messages.Enqueue(message);
	}
}
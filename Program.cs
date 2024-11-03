using ChatAppDB_Test;

namespace ChatAppDB_Test;

public class Program
{
	static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			Server server = new Server();
			server.Operate();
			//Console.WriteLine("Exiting the server");
		}
		else if (args.Length == 1)
		{
			Client client = new Client(args[0]);
			//Console.WriteLine("Exiting the client");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerUDP
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length > 0 && args[0] == "c")
			{
				Client client = new Client();
				client.RunClient("127.0.0.1", RandomGen.RandomString(5));
			}
			else
			{
				Server server = new Server();
				server.RunServer();
			}
		}
		public static void WriteLine(string _data, ConsoleColor c)
		{
			string dateString = $"[{DateTime.Now.ToString("HH:mm:ss")}] ";

			Console.ForegroundColor = c;
			Console.WriteLine(dateString + _data);
			Console.ResetColor();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameServerUDP
{
	class Client
	{
		public UdpUser client;

		public void RunClient(string ip, string nick = "", int Port = 7777)
		{
			
			client = UdpUser.ConnectTo(ip, Port);
			SendPacket(ClientPackets.RegisterPacketClient, nick);
			Task.Factory.StartNew(async () =>
			{
				while (true)
				{
					try
					{
						Received received = await client.Receive();
						CheckPacket(received.Message, received.Sender);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			});

			string read;
			bool yes = true;
			while (yes)
			{
				read = Console.ReadLine();
				client.Send(read);
			}
		}

		public void CheckPacket(string data, IPEndPoint iPEndPoint)
		{
			if (data == "ERROR") return;

			int startPos = data.IndexOf("[") + "[".Length;
			int length = data.IndexOf("]") - startPos;

			if (length > 0)
			{
				string packetName = data.Substring(startPos, length);

				if (Enum.TryParse(typeof(ServerPackets), packetName, out object cp))
				{
					ServerPackets serverPackets = (ServerPackets)cp;
					string pocketMessage = data.Replace($"[{serverPackets}] ", "");

					if(serverPackets == ServerPackets.HeartbeatPacket)
					{
						SendPacket(ClientPackets.HeartBeatCallbackPacket);
					}
					else if(serverPackets == ServerPackets.ChatPacket)
					{
						WriteLine(pocketMessage, ConsoleColor.Yellow);
					}
				}
				else
				{
					WriteLine("Invalid packet!", ConsoleColor.Red);
				}
			}
			else
			{
				WriteLine("ITS NOT A PACKET!", ConsoleColor.Red);
			}
		}

		public void SendPacket(ClientPackets cp, string _data = null) => SendMsg($"[{cp}] {_data}");
		public void SendMsg(string _data) => client.Send(_data);

		public void WriteLine(string _data, ConsoleColor c)
		{
			string dateString = $"[{DateTime.Now.ToString("HH:mm:ss")}] ";

			Console.ForegroundColor = c;
			Console.WriteLine(dateString + _data);
			Console.ResetColor();
		}
	}
}

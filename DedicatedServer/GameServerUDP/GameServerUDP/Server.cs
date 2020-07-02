using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerUDP
{
	class Server
	{
		public UdpListener server;
		public bool IsServerRunning;

		//settings
		public float HearthBeatTime => 1;
		public int MaxConnections => 20;

		//debug settings
		public bool PacketReceivedMessage => false;
		public bool HearthBeatMessage => false;


		public List<NetClient> _clients = new List<NetClient>();
		public List<HeartBeatClient> heartBeatClients = new List<HeartBeatClient>();

		public void RunServer(int port = 7777)
		{
			WriteLine($"Server started on port {port} and ip {IPAddress.Any}.", ConsoleColor.Magenta);

			server = new UdpListener(port);

			IsServerRunning = true;
			ServerHearthBeat();

			Task.Factory.StartNew(async () =>
			{
				while (IsServerRunning)
				{
					var received = await server.Receive();
					ServerReceive(received);
				}
			});

			while (IsServerRunning) { }
		}

		public void ServerHearthBeat()
		{
			new Thread(() =>
			{
				int timing = (int)(HearthBeatTime * 1000);

				while (IsServerRunning)
				{
					Thread.Sleep(timing);
					if(HearthBeatMessage) WriteLine("HeartBeat [+]", ConsoleColor.Cyan);
					foreach (NetClient client in _clients)
					{
						heartBeatClients.Add(new HeartBeatClient(client.IPEndPoint, false));
					}
					BroadcastPacket(ServerPackets.HeartbeatPacket);
					Thread.Sleep(timing);
					if (HearthBeatMessage) WriteLine("HeartBeat [-]", ConsoleColor.Cyan);
					foreach (HeartBeatClient hbc in heartBeatClients.ToArray())
					{
						bool userResponded = false;
						int userIndex = -1;

						for (int i = 0; i < _clients.Count; i++)
						{
							if (hbc.iPEndPoint.Equals(_clients[i].IPEndPoint))
							{
								if (hbc.responded)
								{
									userResponded = true;
								}
								userIndex = i;
								break;
							}
						}

						if (!userResponded)
						{
							BroadcastPacket(ServerPackets.ChatPacket, _clients[userIndex].IPEndPoint, "KICKED FROM SERVER!");

							NetClient client = _clients[userIndex];
							_clients.RemoveAt(userIndex);

							WriteLine($"[-] Player disconnected! ({client.Nick}) ({client.IPEndPoint.Address}:{client.IPEndPoint.Port}) [{_clients.Count}/{MaxConnections}]", ConsoleColor.Red);
							Console.Title = _clients.Count.ToString() + "/" + MaxConnections;
						}
					}
					heartBeatClients.Clear();
				}
			}).Start();
		}

		public void ServerReceive(Received rec)
		{
			CheckPacket(rec.Message, rec.Sender);
		}


		public void CheckPacket(string data, IPEndPoint iPEndPoint)
		{
			if (data == "ERROR") return;

			int startPos = data.IndexOf("[") + "[".Length;
			int length = data.IndexOf("]") - startPos;

			if (length > 0)
			{
				string packetName = data.Substring(startPos, length);

				if (Enum.TryParse(typeof(ClientPackets), packetName, out object cp))
				{
					ClientPackets clientPacket = (ClientPackets)cp;
					string pocketMessage = data.Replace($"[{clientPacket}] ", "");

					if (_clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) > -1)
					{
						if(PacketReceivedMessage) WriteLine($"Recived packet: {clientPacket}, from {iPEndPoint.Address}:{iPEndPoint.Port} with message: {pocketMessage}", ConsoleColor.Green);

						if (clientPacket == ClientPackets.HeartBeatCallbackPacket)
						{
							int index = heartBeatClients.FindIndex(x => x.iPEndPoint.Equals(iPEndPoint));
							if (index > -1)
							{
								heartBeatClients[index] = new HeartBeatClient(iPEndPoint, true);
							}
						}
					}
					else if (_clients.Count < MaxConnections && _clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) < 0)
					{
						if (clientPacket == ClientPackets.RegisterPacketClient)
						{
							if (_clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) < 0)
							{
								_clients.Add(new NetClient(iPEndPoint, pocketMessage));
								WriteLine($"[+] Player connected! ({pocketMessage}) ({iPEndPoint.Address}:{iPEndPoint.Port}) [{_clients.Count}/{MaxConnections}]", ConsoleColor.Green);
								Console.Title = _clients.Count.ToString() + "/" + MaxConnections;
							}
						}
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
				BroadcastPacket(ServerPackets.ChatPacket, "XD");
			}
		}

		public void WriteLine(string _data, ConsoleColor c)
		{
			string dateString = $"[{DateTime.Now.ToString("HH:mm:ss")}] ";

			Console.ForegroundColor = c;
			Console.WriteLine(dateString + _data);
			Console.ResetColor();
		}
		public void WriteLineCentered(string _data, ConsoleColor c)
		{
			Console.ForegroundColor = c;
			Console.WriteLine("{0," + ((Console.WindowWidth / 2) + _data.Length / 2) + "}", _data);
			Console.ResetColor();
		}

		public void BroadcastPacket(ServerPackets sp, string _data = null) => Broadcast($"[{sp}] {_data}");
		public void BroadcastPacket(ServerPackets sp, IPEndPoint iPEndPoint, string _data = null) => Broadcast($"[{sp}] {_data}", iPEndPoint);

		public void Broadcast(string _data)
		{
			foreach (NetClient client in _clients.ToArray())
			{
				server.Send(_data, client.IPEndPoint);
			}
		}
		public void Broadcast(string _data, IPEndPoint iPEndPoint)
		{
			foreach (NetClient client in _clients.ToArray())
			{
				if (client.IPEndPoint.Equals(iPEndPoint))
				{
					server.Send(_data, client.IPEndPoint);
					break;
				}
			}
		}
	}
}

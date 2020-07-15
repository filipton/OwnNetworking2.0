using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
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
		public float HearthBeatTime => 0.5f;
		public int MaxConnections => 20;
		public int TPS = 120;

		//debug settings
		public bool PacketReceivedMessage => false;
		public bool HearthBeatMessage => false;

		int NextPId = 0;


		public List<NetClient> _clients = new List<NetClient>();
		public List<HeartBeatClient> heartBeatClients = new List<HeartBeatClient>();

		public List<PlayerAuthority> playerAuthorities = new List<PlayerAuthority>();

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

							int index = playerAuthorities.FindIndex(x => x.Nick == client.Nick);
							if(index > -1)
							{
								BroadcastPacket(ServerPackets.DeSpawnPlayerPacket, playerAuthorities[index].playerId.ToString());
								playerAuthorities.RemoveAt(index);
							}

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
				if (int.TryParse(data.Substring(startPos, length), out int packetId))
				{
					ClientPackets clientPacket = (ClientPackets)packetId;
					string pocketMessage = data.Replace($"[{packetId}] ", "");

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
						else if (clientPacket == ClientPackets.PlayerNetworkSyncPacket)
						{
							string[] svectors = pocketMessage.Split(':');

							if (svectors.Length == 6)
							{
								BroadcastPacketExclude(ServerPackets.NetworkSyncPacket, iPEndPoint, pocketMessage);
							}
							else
							{
								WriteLine($"An error occured when parsing packet: {ClientPackets.PlayerNetworkSyncPacket}!", ConsoleColor.Red);
							}
						}
					}
					else if (_clients.Count < MaxConnections && _clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) < 0)
					{
						if (clientPacket == ClientPackets.RegisterPacketClient)
						{
							_clients.Add(new NetClient(iPEndPoint, pocketMessage));

							foreach(PlayerAuthority pa in playerAuthorities)
							{
								BroadcastPacket(ServerPackets.SpawnPlayerPacket, iPEndPoint, pa.playerId.ToString());
							}

							playerAuthorities.Add(new PlayerAuthority(pocketMessage, NextPId));

							BroadcastPacket(ServerPackets.RegisterPacket, iPEndPoint, NextPId.ToString());
							BroadcastPacket(ServerPackets.SpawnPlayerPacket, (NextPId++).ToString());

							WriteLine($"[+] Player connected! ({pocketMessage}) ({iPEndPoint.Address}:{iPEndPoint.Port}) [{_clients.Count}/{MaxConnections}]", ConsoleColor.Green);

							Console.Title = _clients.Count.ToString() + "/" + MaxConnections;
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

		public void BroadcastPacket(ServerPackets sp, string _data = null) => Broadcast($"[{(int)sp}] {_data}");
		public void BroadcastPacket(ServerPackets sp, IPEndPoint iPEndPoint, string _data = null) => Broadcast($"[{(int)sp}] {_data}", iPEndPoint);
		public void BroadcastPacketExclude(ServerPackets sp, IPEndPoint iPEndPoint, string _data = null) => BroadcastExclude($"[{(int)sp}] {_data}", iPEndPoint);

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
		public void BroadcastExclude(string _data, IPEndPoint iPEndPoint)
		{
			foreach (NetClient client in _clients.ToArray())
			{
				if (!client.IPEndPoint.Equals(iPEndPoint))
				{
					server.Send(_data, client.IPEndPoint);
				}
			}
		}
	}
}

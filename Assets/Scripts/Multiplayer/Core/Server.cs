using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Server : NetworkBehavoiur
{
	public float HearthBeatTime = 0.5f;
	public bool PacketReceivedMessage = false;
	public bool HearthBeatMessage = false;

	int NextPId = 0;
	[HideInInspector] public List<NetClient> _clients = new List<NetClient>();
	[HideInInspector] public List<HeartBeatClient> heartBeatClients = new List<HeartBeatClient>();
	[HideInInspector] public List<PlayerAuthority> playerAuthorities = new List<PlayerAuthority>();

	UdpListener server;


	/// <summary>
	/// Runs server instance
	/// </summary>
	/// <param name="port">Port of server instance</param>
	/// <returns></returns>
	public void RunServer(int port = 7777)
	{
		WriteLine($"Server started on port {port} and ip {IPAddress.Any}.", Color.magenta);
		NetworkManager.singleton.IsServerRunning = true;

		server = new UdpListener(port);

		NetworkManager.singleton.IsServerRunning = true;
		ServerHearthBeat();

		Task.Factory.StartNew(async () =>
		{
			while (NetworkManager.singleton.IsServerRunning)
			{
				var received = await server.Receive();
				ServerReceive(received);
			}
		});
	}

	void ServerHearthBeat()
	{
		new Thread(() =>
		{
			int timing = (int)(HearthBeatTime * 1000);

			while (NetworkManager.singleton.IsServerRunning)
			{
				Thread.Sleep(timing);
				if (HearthBeatMessage) WriteLine("HeartBeat [+]", Color.cyan);
				foreach (NetClient client in _clients)
				{
					heartBeatClients.Add(new HeartBeatClient(client.IPEndPoint, false));
				}
				BroadcastPacket(ServerPackets.HeartbeatPacket);
				Thread.Sleep(timing);
				if (HearthBeatMessage) WriteLine("HeartBeat [-]", Color.cyan);
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

						if (IsServerOnly())
						{
							int ind2 = NetworkManager.singleton.networkPlayers.FindIndex(x => x.ObjectId == playerAuthorities[index].playerId);
							NetworkPlayer nId = NetworkManager.singleton.networkPlayers[ind2];
							if (ind2 > -1)
							{
								ThreadManager.ExecuteOnMainThread(() =>
								{
									NetworkManager.singleton.networkPlayers.RemoveAt(ind2);
									Destroy(nId.PlayerRoot.gameObject);
								});
							}
							else
							{
								WriteLine("This object does not exists!", Color.red);
							}
						}

						if (index > -1)
						{
							BroadcastPacket(ServerPackets.DeSpawnPlayerPacket, playerAuthorities[index].playerId.ToString());
							playerAuthorities.RemoveAt(index);
						}

						WriteLine($"[-] Player disconnected! ({client.Nick}) ({client.IPEndPoint.Address}:{client.IPEndPoint.Port}) [{_clients.Count}/{NetworkManager.singleton.MaxConnections}]", Color.red);
						Console.Title = _clients.Count.ToString() + "/" + NetworkManager.singleton.MaxConnections;
					}
				}
				heartBeatClients.Clear();
			}
		}).Start();
	}

	void ServerReceive(Received rec)
	{
		CheckServerPacket(rec.Message, rec.Sender);
	}


	void CheckServerPacket(string data, IPEndPoint iPEndPoint)
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
					if (PacketReceivedMessage) WriteLine($"Recived packet: {clientPacket}, from {iPEndPoint.Address}:{iPEndPoint.Port} with message: {pocketMessage}", Color.green);

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

							if (IsServerOnly())
							{
								int netId = PacketParser.ParseNetworkSyncPacket(pocketMessage, out Vector3 pos, out float _rotX, out float _rotY);
								int index = NetworkManager.singleton.networkPlayers.FindIndex(x => x.ObjectId == netId);

								if (index > -1)
								{
									NetworkPlayer nId = NetworkManager.singleton.networkPlayers[index];

									ThreadManager.ExecuteOnMainThread(() =>
									{
										nId.PlayerRoot.position = pos;
										nId.Camera.transform.localRotation = Quaternion.Euler(_rotX, 0, 0);
										nId.PlayerHead.localRotation = Quaternion.Euler(_rotX, 0, 0);
										nId.PlayerRoot.rotation = Quaternion.Euler(0, _rotY, 0);
									});
								}
								else
								{
									WriteLine("This object does not exists!", Color.red);
								}
							}
						}
						else
						{
							WriteLine($"An error occured when parsing packet: {ClientPackets.PlayerNetworkSyncPacket}!", Color.red);
						}
					}
				}
				else if (_clients.Count < NetworkManager.singleton.MaxConnections && _clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) < 0)
				{
					if (clientPacket == ClientPackets.RegisterPacketClient)
					{
						_clients.Add(new NetClient(iPEndPoint, pocketMessage));

						foreach (PlayerAuthority pa in playerAuthorities)
						{
							BroadcastPacket(ServerPackets.SpawnPlayerPacket, iPEndPoint, pa.playerId.ToString());
						}

						playerAuthorities.Add(new PlayerAuthority(pocketMessage, NextPId));

						if (IsServerOnly())
						{
							int ptc = NextPId;

							ThreadManager.ExecuteOnMainThread(() =>
							{
								GameObject playerP = Instantiate(NetworkManager.singleton.playerPrefab);
								playerP.GetComponent<NetworkPlayer>().ObjectId = ptc;
							});
						}

						BroadcastPacket(ServerPackets.RegisterPacket, iPEndPoint, NextPId.ToString());
						BroadcastPacket(ServerPackets.SpawnPlayerPacket, NextPId.ToString());

						NextPId += 1;

						WriteLine($"[+] Player connected! ({pocketMessage}) ({iPEndPoint.Address}:{iPEndPoint.Port}) [{_clients.Count}/{NetworkManager.singleton.MaxConnections}]", Color.green);

						Console.Title = _clients.Count.ToString() + "/" + NetworkManager.singleton.MaxConnections;
					}
				}
			}
			else
			{
				WriteLine("Invalid packet!", Color.red);
			}
		}
		else
		{
			WriteLine("ITS NOT A PACKET!", Color.red);
			BroadcastPacket(ServerPackets.ChatPacket, "XD");
		}
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
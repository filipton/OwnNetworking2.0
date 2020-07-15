using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager singleton;

	[Header("Connection Settings")]
	public string Ip = "127.0.0.1";
	public int Port = 7777;

	[Scene] public string offlineScene = "";
	[Scene] public string onlineScene = "";

	[Header("Server Settings")]
	public GameObject playerPrefab;
	public float HearthBeatTime = 0.5f;
	public int MaxConnections = 20;
	public bool PacketReceivedMessage = false;
	public bool HearthBeatMessage = false;

	[HideInInspector] public bool IsClientRunning;
	[HideInInspector] public bool IsServerRunning;


	//CLIENT FIELDS
	[HideInInspector] public int LocalPId = -1;
	[HideInInspector] public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();


	//SERVER FIELDS
	int NextPId = 0;
	[HideInInspector] public List<NetClient> _clients = new List<NetClient>();
	[HideInInspector] public List<HeartBeatClient> heartBeatClients = new List<HeartBeatClient>();
	[HideInInspector] public List<PlayerAuthority> playerAuthorities = new List<PlayerAuthority>();

	UdpUser client;
	UdpListener server;

	// Start is called before the first frame update
	void Start()
	{
		DontDestroyOnLoad(this);
		singleton = this;
		if (IsHeadlessMode())
		{
			RunServer(Port);
			SceneManager.LoadScene(onlineScene);
		}
	}

	private void OnApplicationQuit()
	{
		IsClientRunning = false;
		IsServerRunning = false;
	}

	public bool IsServerOnly() => IsServerRunning && !IsClientRunning;
	public bool IsClientOnly() => IsClientRunning && !IsServerRunning;


	#region Client
	public void RunClient(string ip, string nick = "", int Port = 7777)
	{
		IsClientRunning = true;

		client = UdpUser.ConnectTo(ip, Port);
		SendPacket(ClientPackets.RegisterPacketClient, nick);
		Task.Factory.StartNew(async () =>
		{
			while (IsClientRunning)
			{
				try
				{
					Received received = await client.Receive();
					CheckClientPacket(received.Message, received.Sender);
				}
				catch (Exception ex)
				{
					WriteLine(ex.ToString(), Color.red);
				}
			}
		});
	}

	public void CheckClientPacket(string data, IPEndPoint iPEndPoint)
	{
		//print("XDX");

		if (data == "ERROR") return;

		int startPos = data.IndexOf("[") + "[".Length;
		int length = data.IndexOf("]") - startPos;

		if (length > 0)
		{
			if (int.TryParse(data.Substring(startPos, length), out int packetId))
			{
				ServerPackets serverPackets = (ServerPackets)packetId;
				string pocketMessage = data.Replace($"[{packetId}] ", "");

				if (serverPackets == ServerPackets.HeartbeatPacket)
				{
					SendPacket(ClientPackets.HeartBeatCallbackPacket);
				}
				else if (serverPackets == ServerPackets.RegisterPacket)
				{
					int pId = int.Parse(pocketMessage);

					LocalPId = pId;
				}
				else if (serverPackets == ServerPackets.NetworkSyncPacket)
				{
					int netId = PacketParser.ParseNetworkSyncPacket(pocketMessage, out Vector3 pos, out float _rotX, out float _rotY);
					int index = networkPlayers.FindIndex(x => x.ObjectId == netId);

					if (index > -1)
					{
						NetworkPlayer nId = networkPlayers[index];

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
				else if (serverPackets == ServerPackets.SpawnPlayerPacket)
				{
					int pId = int.Parse(pocketMessage);
					ThreadManager.ExecuteOnMainThread(() =>
					{
						GameObject playerP = Instantiate(playerPrefab);
						playerP.GetComponent<NetworkPlayer>().ObjectId = pId;

						if (pId == LocalPId)
						{
							playerP.GetComponent<PlayerMovement>().enabled = true;
							playerP.GetComponentInChildren<Camera>().enabled = true;
							playerP.GetComponentInChildren<PlayerMouseLook>().enabled = true;
						}
					});
				}
				else if (serverPackets == ServerPackets.DeSpawnPlayerPacket)
				{
					int netId = int.Parse(pocketMessage);
					int index = networkPlayers.FindIndex(x => x.ObjectId == netId);

					NetworkPlayer nId = networkPlayers[index];

					if (index > -1)
					{
						ThreadManager.ExecuteOnMainThread(() =>
						{
							networkPlayers.RemoveAt(index);
							Destroy(nId.PlayerRoot.gameObject);
						});
					}
					else
					{
						WriteLine("This object does not exists!", Color.red);
					}
				}
				else if (serverPackets == ServerPackets.TpPlayerPacket)
				{
					int netId = PacketParser.ParseNetworkSyncPacket(pocketMessage, out Vector3 pos, out float _rotX, out float _rotY);
					int index = networkPlayers.FindIndex(x => x.ObjectId == netId);

					if (index > -1)
					{
						NetworkPlayer nId = networkPlayers[index];

						ThreadManager.ExecuteOnMainThread(() =>
						{
							bool b = nId.GetComponent<PlayerMovement>().enabled;
							if (b) nId.GetComponent<PlayerMovement>().enabled = false;

							nId.PlayerRoot.position = pos;
							nId.Camera.transform.localRotation = Quaternion.Euler(_rotX, 0, 0);
							nId.PlayerHead.localRotation = Quaternion.Euler(_rotX, 0, 0);
							nId.PlayerRoot.rotation = Quaternion.Euler(0, _rotY, 0);

							if (b) nId.GetComponent<PlayerMovement>().enabled = true;
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
				WriteLine("Invalid packet!", Color.red);
			}
		}
		else
		{
			WriteLine("ITS NOT A PACKET!", Color.red);
		}
	}

	public void SendPacket(ClientPackets cp, string _data = null) => SendMsg($"[{(int)cp}] {_data}");
	public void SendMsg(string _data) => client.Send(_data);

	public void WriteLine(string _data, Color c)
	{
		Debug.Log($"<b><color=#{ColorUtility.ToHtmlStringRGB(c)}>{_data}</color></b>");
	}

	#endregion

	#region Server
	public void RunServer(int port = 7777)
	{
		WriteLine($"Server started on port {port} and ip {IPAddress.Any}.", Color.magenta);
		IsServerRunning = true;

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
	}

	public void ServerHearthBeat()
	{
		new Thread(() =>
		{
			int timing = (int)(HearthBeatTime * 1000);

			while (IsServerRunning)
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
							int ind2 = networkPlayers.FindIndex(x => x.ObjectId == playerAuthorities[index].playerId);
							NetworkPlayer nId = networkPlayers[ind2];
							if (ind2 > -1)
							{
								ThreadManager.ExecuteOnMainThread(() =>
								{
									networkPlayers.RemoveAt(ind2);
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

						WriteLine($"[-] Player disconnected! ({client.Nick}) ({client.IPEndPoint.Address}:{client.IPEndPoint.Port}) [{_clients.Count}/{MaxConnections}]", Color.red);
						Console.Title = _clients.Count.ToString() + "/" + MaxConnections;
					}
				}
				heartBeatClients.Clear();
			}
		}).Start();
	}

	public void ServerReceive(Received rec)
	{
		CheckServerPacket(rec.Message, rec.Sender);
	}


	public void CheckServerPacket(string data, IPEndPoint iPEndPoint)
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
								int index = networkPlayers.FindIndex(x => x.ObjectId == netId);

								if (index > -1)
								{
									NetworkPlayer nId = networkPlayers[index];

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
				else if (_clients.Count < MaxConnections && _clients.FindIndex(x => x.IPEndPoint.Equals(iPEndPoint)) < 0)
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
								GameObject playerP = Instantiate(playerPrefab);
								playerP.GetComponent<NetworkPlayer>().ObjectId = ptc;
							});
						}

						BroadcastPacket(ServerPackets.RegisterPacket, iPEndPoint, NextPId.ToString());
						BroadcastPacket(ServerPackets.SpawnPlayerPacket, NextPId.ToString());

						NextPId += 1;

						WriteLine($"[+] Player connected! ({pocketMessage}) ({iPEndPoint.Address}:{iPEndPoint.Port}) [{_clients.Count}/{MaxConnections}]", Color.green);

						Console.Title = _clients.Count.ToString() + "/" + MaxConnections;
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
	#endregion

	public static bool IsHeadlessMode()
	{
		return UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
	}
}
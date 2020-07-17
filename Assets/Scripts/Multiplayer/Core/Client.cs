using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class Client : NetworkBehavoiur
{
	UdpUser client;


	/// <summary>
	/// Runs client instance
	/// </summary>
	/// <param name="ip">Ip to connect</param>
	/// <param name="nick">Nickname for connection</param>
	/// <param name="Port">Port to connect</param>
	/// <returns></returns>
	public void RunClient(string ip, string nick = "", int Port = 7777)
	{
		NetworkManager.singleton.IsClientRunning = true;

		client = UdpUser.ConnectTo(ip, Port);
		SendPacket(ClientPackets.RegisterPacketClient, nick);
		Task.Factory.StartNew(async () =>
		{
			while (NetworkManager.singleton.IsClientRunning)
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

	void CheckClientPacket(string data, IPEndPoint iPEndPoint)
	{
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

					NetworkManager.singleton.LocalPId = pId;
				}
				else if (serverPackets == ServerPackets.NetworkSyncPacket)
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
				else if (serverPackets == ServerPackets.SpawnPlayerPacket)
				{
					int pId = int.Parse(pocketMessage);
					ThreadManager.ExecuteOnMainThread(() =>
					{
						GameObject playerP = Instantiate(NetworkManager.singleton.playerPrefab);
						playerP.GetComponent<NetworkPlayer>().ObjectId = pId;

						if (pId == NetworkManager.singleton.LocalPId)
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
					int index = NetworkManager.singleton.networkPlayers.FindIndex(x => x.ObjectId == netId);

					NetworkPlayer nId = NetworkManager.singleton.networkPlayers[index];

					if (index > -1)
					{
						ThreadManager.ExecuteOnMainThread(() =>
						{
							NetworkManager.singleton.networkPlayers.RemoveAt(index);
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
					int index = NetworkManager.singleton.networkPlayers.FindIndex(x => x.ObjectId == netId);

					if (index > -1)
					{
						NetworkPlayer nId = NetworkManager.singleton.networkPlayers[index];

						ThreadManager.ExecuteOnMainThread(() =>
						{
							if (nId.IsLocalPlayer()) nId.GetComponent<CharacterController>().enabled = false;

							nId.PlayerRoot.position = pos;
							nId.Camera.transform.localRotation = Quaternion.Euler(_rotX, 0, 0);
							nId.PlayerHead.localRotation = Quaternion.Euler(_rotX, 0, 0);
							nId.PlayerRoot.rotation = Quaternion.Euler(0, _rotY, 0);

							if (nId.IsLocalPlayer()) nId.GetComponent<CharacterController>().enabled = true;

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
}
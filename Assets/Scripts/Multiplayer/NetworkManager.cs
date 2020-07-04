using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager singleton;
	public bool IsClientRunning;

	public GameObject playerPrefab;

	public int LocalPId;

	public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();

	UdpUser client;

	// Start is called before the first frame update
	void Start()
	{
		singleton = this;
		IsClientRunning = true;
		RunClient("192.168.1.107", UnityEngine.Random.Range(0, 10000).ToString());
	}

	private void OnApplicationQuit()
	{
		IsClientRunning = false;
	}


	public void RunClient(string ip, string nick = "", int Port = 7777)
	{
		client = UdpUser.ConnectTo(ip, Port);
		SendPacket(ClientPackets.RegisterPacketClient, nick);
		Task.Factory.StartNew(async () =>
		{
			while (IsClientRunning)
			{
				try
				{
					Received received = await client.Receive();
					CheckPacket(received.Message, received.Sender);
				}
				catch (Exception ex)
				{
					WriteLine(ex.ToString(), Color.red);
				}
			}
		});
	}

	public void CheckPacket(string data, IPEndPoint iPEndPoint)
	{
		//print("XDX");

		if (data == "ERROR") return;

		int startPos = data.IndexOf("[") + "[".Length;
		int length = data.IndexOf("]") - startPos;

		if (length > 0)
		{
			string packetName = data.Substring(startPos, length);

			if (Enum.TryParse(packetName, out ServerPackets cp))
			{
				ServerPackets serverPackets = cp;
				string pocketMessage = data.Replace($"[{serverPackets}] ", "");

				if (serverPackets == ServerPackets.HeartbeatPacket)
				{
					SendPacket(ClientPackets.HeartBeatCallbackPacket);
				}
				else if(serverPackets == ServerPackets.RegisterPacket)
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
						WriteLine(pos.ToString(), Color.yellow);
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
				else if(serverPackets == ServerPackets.SpawnPlayerPacket)
				{
					int pId = int.Parse(pocketMessage);
					ThreadManager.ExecuteOnMainThread(() =>
					{
						GameObject playerP = Instantiate(playerPrefab);
						playerP.GetComponent<NetworkPlayer>().ObjectId = pId;

						if(pId == LocalPId)
						{
							playerP.GetComponent<PlayerMovement>().enabled = true;
							playerP.GetComponentInChildren<Camera>().enabled = true;
							playerP.GetComponentInChildren<PlayerMouseLook>().enabled = true;
						}
					});
				}
				else if(serverPackets == ServerPackets.DeSpawnPlayerPacket)
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

	public void SendPacket(ClientPackets cp, string _data = null) => SendMsg($"[{cp}] {_data}");
	public void SendMsg(string _data) => client.Send(_data);

	public void WriteLine(string _data, Color c)
	{
		Debug.Log($"<b><color=#{ColorUtility.ToHtmlStringRGB(c)}>{_data}</color></b>");
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum ServerPackets
{
	RegisterPacket,
	ChatPacket,
	HeartbeatPacket,
	MovementPacket
}

public enum ClientPackets
{
	RegisterPacketClient,
	ChatPacketClient,
	HeartBeatCallbackPacket,
	PlayerMovementPacket
}

public class NetManager : MonoBehaviour
{
	public static NetManager singleton;

	public bool IsClientRunning;
	public Image img;
	UdpUser client;

	// Start is called before the first frame update
	void Start()
    {
		singleton = this;
		IsClientRunning = true;
		RunClient("192.168.1.107", "MOJ STARY");
    }

    // Update is called once per frame
    void Update()
    {
        
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
					print(ex);
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
				else if (serverPackets == ServerPackets.MovementPacket)
				{
					string[] svectors = pocketMessage.Split(':');
					float[] vectors = Array.ConvertAll(svectors, float.Parse);


					ThreadManager.ExecuteOnMainThread(() =>
					{
						img.transform.localPosition = new Vector3(vectors[0], vectors[1], vectors[2]);
					});
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

		Debug.Log(dateString + _data);
	}
}
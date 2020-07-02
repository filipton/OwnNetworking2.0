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
	HeartbeatPacket
}

public enum ClientPackets
{
	RegisterPacketClient,
	ChatPacketClient,
	HeartBeatCallbackPacket
}

public class NetManager : MonoBehaviour
{
	public bool IsClientRunning;
	public Image img;
	UdpUser client;

	// Start is called before the first frame update
	void Start()
    {
		IsClientRunning = true;
		RunClient("127.0.0.1", "MOJ STARY");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnApplicationQuit()
	{
		print("YOS");
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
				else if (serverPackets == ServerPackets.ChatPacket)
				{
					WriteLine(pocketMessage, ConsoleColor.Yellow);
					ThreadManager.ExecuteOnMainThread(() =>
					{
						img.name = pocketMessage;
						img.color = UnityEngine.Random.ColorHSV();
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
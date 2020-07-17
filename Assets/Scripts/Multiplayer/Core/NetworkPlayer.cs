using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class NetworkPlayer : NetworkIdentity
{
    public Transform PlayerRoot;
    public Transform PlayerHead;
    public Camera Camera;

	public Canvas canvas;
	public TextMeshProUGUI roundTimeText;

	public bool IsEnded = false;
	Stopwatch roundTimer = new Stopwatch();

	private void Start()
	{
		NetworkManager.singleton.networkPlayers.Add(this);

		if (IsLocalPlayer())
		{
			canvas.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (IsLocalPlayer())
		{
			NetworkManager.singleton.SendPacket(ClientPackets.PlayerNetworkSyncPacket, PacketParser.NetworkSyncPacketToString(ObjectId, PlayerRoot.position, new Vector3(Camera.transform.eulerAngles.x, PlayerRoot.eulerAngles.y, 0)));

			roundTimeText.text = roundTimer.Elapsed.ToString();
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.gameObject.name == "FLOOR")
		{
			if (IsServer())
			{
				NetworkManager.singleton.BroadcastPacket(ServerPackets.TpPlayerPacket, PacketParser.NetworkSyncPacketToString(ObjectId, Vector3.zero + Vector3.up, Vector3.zero));
			}
		}
		else if (other.gameObject.name == "EndTrigger")
		{
			if (!IsEnded)
			{
				roundTimer.Stop();
				print($"END TIME: {roundTimer.Elapsed}");
			}
			IsEnded = true;

			if (IsServer())
			{
				NetworkManager.singleton.BroadcastPacket(ServerPackets.TpPlayerPacket, PacketParser.NetworkSyncPacketToString(ObjectId, Vector3.zero + Vector3.up, Vector3.zero));
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.gameObject.name == "StartTrigger")
		{
			roundTimer.Reset();
			roundTimer.Start();
			IsEnded = false;
		}
	}
}
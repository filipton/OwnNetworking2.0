using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkIdentity
{
    public Transform PlayerRoot;
    public Transform PlayerHead;
    public Camera Camera;

	private void Start()
	{
		NetworkManager.singleton.networkPlayers.Add(this);
	}

	private void FixedUpdate()
	{
		if (IsLocalPlayer())
		{
			NetworkManager.singleton.SendPacket(ClientPackets.PlayerNetworkSyncPacket, PacketParser.NetworkSyncPacketToString(ObjectId, PlayerRoot.position, new Vector3(Camera.transform.eulerAngles.x, PlayerRoot.eulerAngles.y, 0)));
		}
	}

	void OnTriggerStay(Collider other)
	{
		if(IsServer() && other.gameObject.name == "FLOOR")
		{
			NetworkManager.singleton.BroadcastPacket(ServerPackets.TpPlayerPacket, PacketParser.NetworkSyncPacketToString(ObjectId, Vector3.zero + Vector3.up, Vector3.zero));
		}
	}
}
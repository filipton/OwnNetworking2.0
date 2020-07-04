using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public int ObjectId;

    public Transform PlayerRoot;
    public Transform PlayerHead;
    public Camera Camera;

	private void Start()
	{
		NetworkManager.singleton.networkPlayers.Add(this);
	}

	private void Update()
	{
		if (ObjectId == NetworkManager.singleton.LocalPId)
		{
			NetworkManager.singleton.SendPacket(ClientPackets.PlayerNetworkSyncPacket, PacketParser.NetworkSyncPacketToString(ObjectId, PlayerRoot.position, new Vector3(Camera.transform.eulerAngles.x, PlayerRoot.eulerAngles.y, 0)));
		}
	}
}
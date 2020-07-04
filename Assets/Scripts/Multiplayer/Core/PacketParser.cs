using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketParser
{
    public static int ParseNetworkSyncPacket(string pocketMsg, out Vector3 position, out float rotX, out float rotY)
	{
        string[] msgParts = pocketMsg.Split(':');

        position = new Vector3(float.Parse(msgParts[1]), float.Parse(msgParts[2]), float.Parse(msgParts[3]));
        rotX = float.Parse(msgParts[4]);
        rotY = float.Parse(msgParts[5]);

        return int.Parse(msgParts[0]);
    }

    public static string NetworkSyncPacketToString(int id, Vector3 position, Vector3 eularAngles)
    {
        return $"{id}:{position.x}:{position.y}:{position.z}:{eularAngles.x}:{eularAngles.y}";
    }
}


public enum ServerPackets
{
    RegisterPacket,
    ChatPacket,
    HeartbeatPacket,
    NetworkSyncPacket,
    SpawnPlayerPacket,
    DeSpawnPlayerPacket
}

public enum ClientPackets
{
    RegisterPacketClient,
    ChatPacketClient,
    HeartBeatCallbackPacket,
    PlayerNetworkSyncPacket
}
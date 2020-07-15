using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerUDP
{
    public enum ServerPackets
    {
        RegisterPacket = 1,
        ChatPacket,
        HeartbeatPacket,
        NetworkSyncPacket,
        SpawnPlayerPacket,
        DeSpawnPlayerPacket
    }

    public enum ClientPackets
    {
        RegisterPacketClient = 1,
        ChatPacketClient,
        HeartBeatCallbackPacket,
        PlayerNetworkSyncPacket
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerUDP
{
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
}

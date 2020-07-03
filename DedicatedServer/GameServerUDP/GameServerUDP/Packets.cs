using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerUDP
{
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
}

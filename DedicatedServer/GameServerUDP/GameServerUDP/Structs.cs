using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GameServerUDP
{
    public struct NetClient
    {
        public IPEndPoint IPEndPoint;
        public string Nick;

        public NetClient(IPEndPoint iep, string nick)
        {
            IPEndPoint = iep;
            Nick = nick;
        }
    }

    public struct HeartBeatClient
    {
        public IPEndPoint iPEndPoint;
        public bool responded;

        public HeartBeatClient(IPEndPoint iep, bool res)
        {
            iPEndPoint = iep;
            responded = res;
        }
    }

    public struct Received
    {
        public IPEndPoint Sender;
        public string Message;
    }
}

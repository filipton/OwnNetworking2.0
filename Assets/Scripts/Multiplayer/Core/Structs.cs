using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

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

public struct PlayerAuthority
{
    public string Nick;
    public int playerId;

    public PlayerAuthority(string nick, int pid)
    {
        Nick = nick;
        playerId = pid;
    }
}

public struct Received
{
    public IPEndPoint Sender;
    public string Message;
}

public class SceneAttribute : PropertyAttribute { }
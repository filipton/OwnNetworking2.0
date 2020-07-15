using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

abstract class UdpBase
{
    protected UdpClient Client;

    protected UdpBase()
    {
        Client = new UdpClient();
    }

    public async Task<Received> Receive()
    {
        try
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }
        catch { return new Received() { Message = "ERROR", Sender = new IPEndPoint(IPAddress.Any, 0) }; } //after disconecting player
    }
}

//Server
class UdpListener : UdpBase
{
    private IPEndPoint _listenOn;

    public UdpListener(int port)
    {
        _listenOn = new IPEndPoint(IPAddress.Any, port);
        Client = new UdpClient(_listenOn);
    }

    public UdpListener(IPEndPoint endpoint)
    {
        _listenOn = endpoint;
        Client = new UdpClient(_listenOn);
    }

    public void Send(string message, IPEndPoint endpoint)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length, endpoint);
    }
}


//Client
class UdpUser : UdpBase
{
    private UdpUser() { }

    public static UdpUser ConnectTo(string hostname, int port)
    {
        var connection = new UdpUser();
        connection.Client.Connect(hostname, port);
        return connection;
    }

    public void Send(string message)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length);
    }
}
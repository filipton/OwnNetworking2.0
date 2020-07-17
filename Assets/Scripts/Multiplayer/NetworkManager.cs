using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : NetworkBehavoiur
{
	public static NetworkManager singleton;

	[Header("Connection Settings")]
	public string Ip = "127.0.0.1";
	public int Port = 7777;

	[Scene] public string offlineScene = "";
	[Scene] public string onlineScene = "";

	[Header("Server Settings")]
	public GameObject playerPrefab;
	public int MaxConnections = 20;

	[HideInInspector] public bool IsClientRunning;
	[HideInInspector] public bool IsServerRunning;

	[HideInInspector] public int LocalPId = -1;
	[HideInInspector] public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();

	public Client Client;
	public Server Server;

	// Start is called before the first frame update
	void Start()
	{
		DontDestroyOnLoad(this);
		singleton = this;
		if (IsHeadlessMode())
		{
			RunServer();
			SceneManager.LoadScene(onlineScene);
		}
	}

	private void OnApplicationQuit()
	{
		IsClientRunning = false;
		IsServerRunning = false;
	}

	public void RunServer(int port = 7777)
	{
		Server = gameObject.AddComponent<Server>();
		Server.RunServer(port);
	}

	public void RunClient(string ip, string nick = "", int Port = 7777)
	{
		Client = gameObject.AddComponent<Client>();
		Client.RunClient(ip, nick, Port);
	}
}
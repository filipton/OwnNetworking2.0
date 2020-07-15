using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkIdentity : MonoBehaviour
{
    public int ObjectId;

	public bool IsLocalPlayer() => ObjectId == NetworkManager.singleton.LocalPId;
	public bool IsClient() => NetworkManager.singleton.IsClientRunning;
	public bool IsServer() => NetworkManager.singleton.IsServerRunning;
	public bool IsClientOnly() => NetworkManager.singleton.IsClientRunning && !NetworkManager.singleton.IsServerRunning;
	public bool IsServerOnly() => NetworkManager.singleton.IsServerRunning && !NetworkManager.singleton.IsClientRunning;
}
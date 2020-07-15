using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void StartClient()
	{
		NetworkManager.singleton.RunClient(NetworkManager.singleton.Ip, UnityEngine.Random.Range(0, 10000).ToString(), NetworkManager.singleton.Port);
		SceneManager.LoadScene(NetworkManager.singleton.onlineScene);
	}

	public void StartServer(bool changeScene = true)
	{
		NetworkManager.singleton.RunServer(NetworkManager.singleton.Port);
		if(changeScene) SceneManager.LoadScene(NetworkManager.singleton.onlineScene);
	}

	public void HostAndPlay()
	{
		StartServer(false);
		StartClient();
	}
}
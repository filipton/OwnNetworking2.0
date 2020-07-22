using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	public List<string> tests = new List<string>();

	public string outs;
	public string ins;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			tests = Serialization_Manager.SerializationManager.LoadObject<List<string>>(ins);
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			outs = Serialization_Manager.SerializationManager.SaveObject(tests);
			ins = outs;
		}
		else if (Input.GetKeyDown(KeyCode.F))
		{
			for(int i = 0; i < 100; i++)
			{
				tests.Add(RandomString(10));
			}
		}
	}

	private static System.Random random = new System.Random();
	public static string RandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
		  .Select(s => s[random.Next(s.Length)]).ToArray());
	}

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
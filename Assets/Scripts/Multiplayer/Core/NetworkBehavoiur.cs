using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic class of network behavoiur
/// </summary>
public class NetworkBehavoiur : MonoBehaviour
{
	public bool IsHeadlessMode()
	{
		return UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
	}

	public bool IsServerOnly() => NetworkManager.singleton.IsServerRunning && !NetworkManager.singleton.IsClientRunning;
	public bool IsClientOnly() => NetworkManager.singleton.IsClientRunning && !NetworkManager.singleton.IsServerRunning;

	public void WriteLine(string _data, Color c)
	{
		Debug.Log($"<b><color=#{ColorUtility.ToHtmlStringRGB(c)}>{_data}</color></b>");
	}
}
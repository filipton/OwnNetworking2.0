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
}
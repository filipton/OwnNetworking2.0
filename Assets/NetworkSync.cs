using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSync : MonoBehaviour
{
    public bool CanMove = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (CanMove)
		{
            NetManager.singleton.SendPacket(ClientPackets.PlayerMovementPacket, $"{transform.localPosition.x}:{transform.localPosition.y}:{transform.localPosition.z}");
		}
    }
}
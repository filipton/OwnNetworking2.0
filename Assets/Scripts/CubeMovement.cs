using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float vert = Input.GetAxis("Vertical");
        float hor = Input.GetAxis("Horizontal");

        this.transform.position += Vector3.right * hor;
        this.transform.position += Vector3.up * vert;

        if (vert != 0 || hor != 0)
		{
            NetManager.singleton.SendPacket(ClientPackets.PlayerMovementPacket, $"{transform.localPosition.x}:{transform.localPosition.y}:{transform.localPosition.z}");
        }
    }
}
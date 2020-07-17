using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Diagnostics;

[System.Serializable]
public struct BM
{
    public Vector3 pos;
    public Vector3 angles;
}

[System.Serializable]
public struct BMs
{
    public List<BM> record;
}

public class PlayerMovement : MonoBehaviour
{
    public bool BotMovement;
    public bool isRecording;

    public int C;

    CharacterController characterController;
    public GameObject GroundCheck;

    public float speed = 7f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f * 2;

    public bool isRunning;
    public bool isGrounded;

    public LayerMask WhatIsGround;
    Vector3 velocity;

    public List<BMs> botMovement = new List<BMs>();
    NetworkPlayer np;

    int botAnim = -1;


    int index = 0;
    int tCounter = 0;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        np = GetComponent<NetworkPlayer>();

		if (BotMovement)
		{
            GetComponentInChildren<PlayerMouseLook>().enabled = false;
        }
    }

    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.H) && !isRecording)
		{
            BotMovement = !BotMovement;
            botAnim = Random.Range(0, botMovement.Count);
            GetComponentInChildren<PlayerMouseLook>().enabled = !BotMovement;
        }
		if (Input.GetKeyDown(KeyCode.R) && !BotMovement)
		{
            isRecording = !isRecording;
            if (isRecording)
            {
                botMovement.Add(new BMs() { record = new List<BM>() });
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) C++;
        if (Input.GetKeyDown(KeyCode.DownArrow)) C--;

		if (BotMovement)
		{
            if(tCounter >= C)
			{
                if (index < botMovement[botAnim].record.Count)
                {
                    np.PlayerRoot.position = botMovement[botAnim].record[index].pos;
                    np.Camera.transform.localRotation = Quaternion.Euler(botMovement[botAnim].record[index].angles.x, 0, 0);
                    np.PlayerHead.localRotation = Quaternion.Euler(botMovement[botAnim].record[index].angles.x, 0, 0);
                    np.PlayerRoot.rotation = Quaternion.Euler(0, botMovement[botAnim].record[index].angles.y, 0);

                    index++;
                }
                else
                {
                    index = 0;
                }
                tCounter = 0;
            }
			else
			{
                tCounter++;
			}
        }
		else
		{
            isGrounded = Physics.CheckSphere(GroundCheck.transform.position, 0.1f, WhatIsGround);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            if (isGrounded)
            {
                isRunning = Input.GetKey(KeyCode.LeftShift);

                x *= isRunning ? 1.5f : 1;
                z *= isRunning ? 1.5f : 1;

                if (z > 0)
                {
                    //animationSync.CmdSetAnimatorBool("WalkingBackwards", false);
                    //animationSync.CmdSetAnimatorBool("Walking", true);
                }
                else if (z < 0)
                {
                    //animationSync.CmdSetAnimatorBool("Walking", false);
                    //animationSync.CmdSetAnimatorBool("WalkingBackwards", true);
                }
                else
                {
                    //animationSync.CmdSetAnimatorBool("Walking", false);
                    //animationSync.CmdSetAnimatorBool("WalkingBackwards", false);
                }
            }

            Vector3 move = transform.right * x + transform.forward * z;

            if (move.magnitude > (isRunning ? 1.5f : 1))
                move = move.normalized * (isRunning ? 1.5f : 1);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }

            velocity.y += gravity * Time.deltaTime;

            characterController.Move(velocity * Time.deltaTime);

            characterController.Move(move * speed * Time.deltaTime);

			if (isRecording)
			{
                BM pos = new BM { pos = np.PlayerRoot.transform.position, angles = new Vector3(np.Camera.transform.eulerAngles.x, np.PlayerRoot.eulerAngles.y, 0) };
                botMovement[botMovement.Count-1].record.Add(pos);
            }
        }
    }
}
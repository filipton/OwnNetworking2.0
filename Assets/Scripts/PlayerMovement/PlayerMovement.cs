using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class PlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    public GameObject GroundCheck;

    public float speed = 7f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f * 2;

    public bool isRunning;

    public LayerMask WhatIsGround;

    Vector3 velocity;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        bool isGrounded = Physics.CheckSphere(GroundCheck.transform.position, 0.1f, WhatIsGround);

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
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMouseLook : MonoBehaviour
{
    public float sensitivityX = 1F;
    public float sensitivityY = 1F;

    public Transform playerBody;

    [HideInInspector] public float rotationX = 0F;
    [HideInInspector] public float rotationY = 0F;

    float minimumX = -360F;
    float maximumX = 360F;
    float minimumY = -90F;
    float maximumY = 90F;

    bool Esc;

    void Start()
    {
        sensitivityX = PlayerPrefs.GetFloat("X", 1);
        sensitivityY = PlayerPrefs.GetFloat("Y", 1);

        CursorManager.RefreshLock("_pdead", true);
    }

    void Update()
    {
        if (!Esc)
        {
            float AxisX = Input.GetAxis("Mouse X") * sensitivityX;
            float AxisY = Input.GetAxis("Mouse Y") * sensitivityY;

            rotationX += AxisX;
            rotationY += AxisY;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

            transform.localRotation = yQuaternion;
            playerBody.localRotation = xQuaternion;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Esc = !Esc;
            CursorManager.RefreshLock("_esc", !Esc);
        }
    }
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
         angle += 360F;
        if (angle > 360F)
         angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
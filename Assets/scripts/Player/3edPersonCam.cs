using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public Transform orientation;
    public Transform player;
    public Transform playerobj;
    public Transform combatlook;
    public Rigidbody rb;
    
    public float rotationspeed;
   
    public CameraStyle currentStyle;

    public GameObject combatcam;
    public GameObject basiccam;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public enum CameraStyle
    {
        Basic,
        Combat
    }

    void Update()
    {
        if (currentStyle == CameraStyle.Basic)
        {
            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;
        
            float horizontalInput = Input.GetAxis("Horizontal");
            float vertivalInput = Input.GetAxis("Vertical");
            Vector3 inputdir = orientation.forward * vertivalInput + orientation.right * horizontalInput;
        
            if (inputdir != Vector3.zero)
            {
                playerobj.forward = Vector3.Slerp(playerobj.forward, inputdir.normalized, Time.deltaTime * rotationspeed);
            }
        }
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLook = combatlook.position - new Vector3(transform.position.x, combatlook.position.y, transform.position.z);
            orientation.forward = dirToCombatLook.normalized;
            playerobj.forward = dirToCombatLook.normalized;
        }

        if (Input.GetKey(KeyCode.Alpha1))
            switchcam(CameraStyle.Basic);

        if (Input.GetKey(KeyCode.Alpha2))
            switchcam(CameraStyle.Combat);

        if (Input.GetKey(KeyCode.Tab))
       {
           Cursor.visible = true;
           Cursor.lockState = CursorLockMode.None;
           //Debug.Log("hi");

        }
        else
       {
           Cursor.visible = false;
           Cursor.lockState = CursorLockMode.Locked;
       }

    }

    void switchcam(CameraStyle newstyle)
    {
        combatcam.SetActive(false);
        basiccam.SetActive(false);

        if (newstyle == CameraStyle.Combat) 
            combatcam.SetActive(true);
        if (newstyle == CameraStyle.Basic)
            basiccam.SetActive(true);

       // currentStyle = newstyle;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class takedownscript : MonoBehaviour
{
    [Header("takedown")]
    public bool takedown;
    public LayerMask Enemy;
    public float num = 1;
    public KeyCode interact = KeyCode.E;

    // Update is called once per frame
    void Update()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, num))
        {
            if (hit.collider.tag != "enemy")
            {
                takedown = false;
            }
            else
            {
                takedown = true;
            }
        }
        else
        {
            off();
        }

        void off()
        {
            takedown = false;
        }

        if (Input.GetKeyDown(interact) && takedown)
        {
            Debug.Log("assasinated");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * num);
    }
}
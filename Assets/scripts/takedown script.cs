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
    RaycastHit hit;
    public GameObject enemy;
    
    void Start()
    {
       
    }

    void Update()
    {
        detect();

        if (Input.GetKeyDown(interact) && takedown)
        {
            Debug.Log("assasinated");
            if (enemy != null)
            {
                enemy.GetComponentInParent<enemyTakedown>().tookdown();
                Debug.Log("enemy is there");
            }
        }
    }

    private void detect()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, num))
        {
            if (hit.collider.tag != "enemy")
            {
                enemy = hit.collider.gameObject;
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
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * num);
    }
}
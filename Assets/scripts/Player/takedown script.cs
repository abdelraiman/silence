using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class takedownscript : MonoBehaviour
{
    [Header("takedown")]
    public bool takedown;
    public LayerMask Enemy;
    public float num; //distance of raycast
    public KeyCode interact = KeyCode.E;
    RaycastHit hit;
    public GameObject enemy;
    public GameObject EIndicator;
    public TextMeshProUGUI input;
    public CloudSave CloudSave;

    void Start()
    {
        EIndicator.SetActive(false);
        input.text = ""+interact;
        
    }

    void Update()
    {
        detect();

        if (Input.GetKeyDown(interact) && takedown)
        {
            Debug.Log("assasinated");
            if (enemy != null)
            {
                CloudSave.addDeath();
                enemy.GetComponentInParent<enemyTakedown>().tookdown();
                enemy.GetComponentInParent<NavMeshAgent>().isStopped=true;
                Debug.Log("enemy killed");
            }
        }
    }

    private void detect()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, num))
        {
            //Debug.Log(hit.transform.name);

            if (hit.collider.tag == "enemy box")
            {
                enemy = hit.collider.gameObject;
                EIndicator.SetActive(true);
                takedown = true;
            }
            else
            {
                takedown = false;
                EIndicator.SetActive(false);
            }
        }
        else
        {
            off();
        }

        void off()
        {
            takedown = false;
            EIndicator.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * num);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent;}
   
    [SerializeField] private string curentstate;
    public path paath;
    [SerializeField] private GameObject player;
    public float sightdistance = 20f;
    public float fov = 85;

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        canseeplayer();
    }

    public bool canseeplayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position,player.transform.position) < sightdistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position;
                float angelToplayer = Vector3.Angle(targetDirection, transform.forward);
                if (angelToplayer >= -fov && angelToplayer <= fov)
                {
                    Ray ray = new Ray(transform.position, targetDirection);
                    Debug.DrawRay(ray.origin,ray.direction * sightdistance);
                }
            }
        }
        return true;
    }
}

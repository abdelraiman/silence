using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private string curentstate;
    [SerializeField] private GameObject player;
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Vector3 LastKnown;


    public float Attackdistance = 4;
    public NavMeshAgent Agent { get => agent; }
    public GameObject Player { get => player; }
    public Vector3 lastKnown { get => LastKnown; set => LastKnown = value; }
    public path paath;

    [Header("sight values")]
    public float sightdistance = 20f;
    public float fov = 60;
    public float eyehight;
    
   
    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        canseeplayer();
        curentstate = stateMachine.activeState.ToString();
   
    }

    public bool InAttackRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= Attackdistance;
    }
    public bool canseeplayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position,player.transform.position) < sightdistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position - (Vector3.up * eyehight);
                float angelToplayer = Vector3.Angle(targetDirection, transform.forward);
                if (angelToplayer >= -fov && angelToplayer <= fov)
                {
                    Ray ray = new Ray(transform.position + (Vector3.up * eyehight), targetDirection);
                    RaycastHit Hitinfo = new RaycastHit();
                    if (Physics.Raycast(ray,out Hitinfo, sightdistance))
                    {
                        if (Hitinfo.transform.gameObject == player)
                        {
                            Debug.DrawRay(ray.origin, ray.direction * sightdistance);
                            //Debug.Log("i have line of sight");
                            
                            return true;
                        }
                    }
                    
                }
            }
        }
        return false;
    }
}

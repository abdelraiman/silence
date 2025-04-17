using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PatrolState : BaseState
{
    public int waypointindex;
    public float wait;

    public override void Enter()
    {
    }
    public override void Preform()
    {
        PatrolCycle();
    }

    public override void Exit()
    {
    }

    public void PatrolCycle() 
    {
        if (enemy.Agent.remainingDistance < 0.2f)
        {
            wait += Time.deltaTime;
            if (wait > 3) 
            {
                if (waypointindex < enemy.paath.waypoints.Count - 1)
                {
                    waypointindex++;
                }
                else
                {
                    waypointindex = 0;
                }
                enemy.Agent.SetDestination(enemy.paath.waypoints[waypointindex].position);
                wait = 0;
            }
            
        }
    }
}

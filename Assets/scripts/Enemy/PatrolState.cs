using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PatrolState : BaseState
{
    public int waypointindex;

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
            if (waypointindex < enemy.paath.waypoints.Count - 1)
            {
                waypointindex++;
            }
            else
            {
                waypointindex = 0;
            }
            enemy.Agent.SetDestination(enemy.paath.waypoints[waypointindex].position);
        }
    }
}

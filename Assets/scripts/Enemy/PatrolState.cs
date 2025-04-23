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
        if (enemy.canseeplayer())
        {
            stateMachine.Changstate(new attackState());
        }
    }

    public override void Exit()
    {
    }

    public void PatrolCycle() 
    {
        enemy.sawplayer = false;
        if (enemy.Agent.remainingDistance < 0.2f)
        {
            wait += Time.deltaTime;
            if (wait > 2) 
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

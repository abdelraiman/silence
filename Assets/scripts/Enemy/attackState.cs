using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackState : BaseState
{
    public float movetimer;
    public float looseplayertimer;
    public override void Enter()
    {   
    }

    public override void Exit()
    {
    }

    public override void Preform()
    {
        if (enemy.canseeplayer())
        {
            enemy.Agent.SetDestination(enemy.lastKnown);
            looseplayertimer = 0;
            movetimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);
            if (movetimer > Random.Range(3,7))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 5));
                movetimer = 0;
            }
            enemy.lastKnown = enemy.Player.transform.position;
        }
        else
        {
            looseplayertimer += Time.deltaTime;
            if (looseplayertimer > 8)
            {
                stateMachine.Changstate(new SeartchState());
            }
        }

        if (enemy.InAttackRange())
        {
            enemy.Agent.isStopped = true;
            //Debug.Log("STOPPPPP!!!");
        }
        else if (enemy.Agent.isStopped == true)
        {
            enemy.Agent.isStopped = false;
            //Debug.Log("moving!!!");
        }

    }
}

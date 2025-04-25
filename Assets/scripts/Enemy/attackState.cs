using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            enemy.playsound();
            enemy.Agent.SetDestination(enemy.lastKnown);
            looseplayertimer = 0;
            movetimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);
            enemy.lastKnown = enemy.Player.transform.position;
            //Debug.Log("A");
        }
        else
        {
            //Debug.Log("B");
            enemy.sawplayer = false;
            looseplayertimer += Time.deltaTime;
            if (looseplayertimer > 8)
            {
                //Debug.Log("D");
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

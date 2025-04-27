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
            HandlePlayerSighted();
        }
        else
        {
            HandlePlayerLost();
        }

        HandleMovement();

        if (enemy.InAttackRange())
        {
            StopAgent();
        }
        else if (enemy.Agent.isStopped)
        {
            ResumeAgent();
        }
    }

    private void HandlePlayerSighted()
    {
        enemy.playsound();
        enemy.Agent.SetDestination(enemy.lastKnown);
        looseplayertimer = 0;
        movetimer += Time.deltaTime;
        enemy.transform.LookAt(enemy.Player.transform);
        enemy.lastKnown = enemy.Player.transform.position;
    }

    private void HandlePlayerLost()
    {
        enemy.sawplayer = false;
        looseplayertimer += Time.deltaTime;

        if (looseplayertimer > 8f)
        {
            stateMachine.Changstate(new SeartchState());
        }
    }

    private void HandleMovement()
    {
        if (!enemy.InAttackRange() && !enemy.Agent.isStopped)
        {
            enemy.Agent.isStopped = false;
        }
    }

    private void StopAgent()
    {
        enemy.Agent.isStopped = true;
    }

    private void ResumeAgent()
    {
        enemy.Agent.isStopped = false;
    }
}

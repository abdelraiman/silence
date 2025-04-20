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
            looseplayertimer = 0;
            movetimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);
            if (movetimer > Random.Range(3,7))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 5));
                movetimer = 0;
            }
        }
        else
        {
            looseplayertimer += Time.deltaTime;
            if (looseplayertimer > 8)
            {
                stateMachine.Changstate(new PatrolState());
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

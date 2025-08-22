using UnityEngine;

public class SeartchState : BaseState
{
    private float seartchTimer;
    public float stoptime = 1;
    public override void Enter()
    {
        enemy.Agent.SetDestination(enemy.lastKnown);
    }

    public override void Exit()
    {

    }

    public override void Preform()
    {
        enemy.sawplayer = false;
        if (enemy.canseeplayer())
        {
            stateMachine.Changstate(new attackState());
        }

        if (enemy.Agent.remainingDistance < enemy.Agent.stoppingDistance)
        {
            seartchTimer += Time.deltaTime;
            if (seartchTimer > Random.Range(5,10))
            {
                Debug.Log("must have been the wind");
                stateMachine.Changstate(new PatrolState());
            }
        }

    }
}

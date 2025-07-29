using UnityEngine.AI;
using UnityEngine;
using System;
public interface ActionStratagy
{
    bool CanPerform { get; }
    bool Complete { get; }

    void Start()
    {

    }

    void Update(float deltatime)
    {

    }

    void Stop()
    {

    }
}

public class MovingStrategy : ActionStratagy
{
    readonly NavMeshAgent agent;
    readonly float WanderRadius;
    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public MovingStrategy(NavMeshAgent agent, float wanderRadius)
    {
        this.agent = agent;
        WanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * WanderRadius).With(y: 0);
            NavMeshHit hit;

            if(NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, WanderRadius,1))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}
public class IdleStrategy : ActionStratagy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly CountDownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountDownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltatime) => timer.Tick(deltatime);
}

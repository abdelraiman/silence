using System;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
    bool CanPerform { get; }
    bool Complete { get; }

    void Start() { }
    void Update(float deltaTime) { }
    void Stop() { }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 0.2f && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public void Start()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destination());
        }
    }

    public void Stop()
    {
        if (agent != null && agent.hasPath)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly float wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 0.2f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius)
    {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            Vector3 candidate = agent.transform.position + randomDirection;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    public void Stop()
    {
        if (agent != null && agent.hasPath)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class AttackStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public AttackStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
    public void Stop() => Complete = true;
}

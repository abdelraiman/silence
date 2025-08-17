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

public class TimedCallbackStrategy : IActionStrategy
{
    readonly CountdownTimer timer;
    readonly System.Action onComplete;

    public bool CanPerform => true;
    public bool Complete { get; private set; }

    public TimedCallbackStrategy(float seconds, System.Action onComplete)
    {
        this.onComplete = onComplete;
        timer = new CountdownTimer(seconds);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => { if (!Complete) onComplete?.Invoke(); Complete = true; };
    }

    public void Start() => timer.Start();
    public void Update(float dt) => timer.Tick(dt);
    public void Stop() => Complete = true;
}

public class RepeatCallbackUntilStrategy : IActionStrategy
{
    readonly float intervalSeconds;
    readonly Func<bool> donePredicate;
    readonly Action onTick;

    CountdownTimer timer;
    public bool CanPerform => true;
    public bool Complete { get; private set; }

    public RepeatCallbackUntilStrategy(float intervalSeconds, Func<bool> donePredicate, Action onTick)
    {
        this.intervalSeconds = intervalSeconds;
        this.donePredicate = donePredicate;
        this.onTick = onTick;
    }
    public void Start()
    {
        Complete = false;
        timer = new CountdownTimer(intervalSeconds);
        timer.OnTimerStop += HandleTick;
        timer.Start();
    }
    public void Update(float deltaTime)
    {
        if (Complete) return;
        timer.Tick(deltaTime);
    }

    void HandleTick()
    {
        if (Complete) return;

        onTick?.Invoke();                 
        if (donePredicate())
        {           
            Complete = true;
            return;
        }

        timer.Start();
    }

    public void Stop() => Complete = true;
}
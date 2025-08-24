using System;
using System.Collections.Generic;
using UnityEngine;

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
    readonly Transform mover;
    readonly Func<Vector3> destinationFn;
    readonly MovementState state;
    readonly float speed;
    readonly float arriveDistance;
    readonly float cornerTolerance;

    List<Vector3> waypoints;
    int index;
    bool started, done;

    public bool CanPerform => true;
    public bool Complete => done;

    public MoveStrategy(Transform mover, Func<Vector3> destinationFn, MovementState state,
                             float speed, float arriveDistance = 0.2f, float cornerTolerance = 0.15f)
    {
        this.mover = mover;
        this.destinationFn = destinationFn;
        this.state = state;
        this.speed = Mathf.Max(0.01f, speed);
        this.arriveDistance = arriveDistance;
        this.cornerTolerance = cornerTolerance;
    }

    public void Start()
    {
        started = true; done = false; index = 0;
        var startPos = mover.position;
        var endPos = destinationFn();

        if (!AStarPathfinder.FindPath(startPos, endPos, out waypoints) || waypoints == null || waypoints.Count == 0)
        {
            done = true;
            if (state != null) { state.HasPath = false; state.IsMoving = false; }
            return;
        }
        if (state != null) { state.HasPath = true; state.IsMoving = true; }
    }

    public void Update(float dt)
    {
        if (!started || done) return;
        if (waypoints == null || index >= waypoints.Count)
        {
            done = true; return;
        }

        var target = waypoints[index];
        var to = target - mover.position;
        to.y = 0f;

        if (to.sqrMagnitude <= cornerTolerance * cornerTolerance)
        {
            index++;
            if (index >= waypoints.Count)
            {
                done = Vector3.SqrMagnitude(mover.position - target) <= arriveDistance * arriveDistance;
                if (done && state != null)
                {
                    state.HasPath = false; state.IsMoving = false;
                }
            }
            return;
        }

        var dir = to.normalized;
        var step = speed * dt;
        var move = dir * Mathf.Min(step, to.magnitude);

        mover.position += move;
        if (move != Vector3.zero) mover.forward = dir;
        if (waypoints != null && waypoints.Count > 1)
        {
            for (int i = 0; i < waypoints.Count - 1; i++)
                Debug.DrawLine(waypoints[i] + Vector3.up * 0.1f,
                               waypoints[i + 1] + Vector3.up * 0.1f,
                               Color.green, 0f);

            if (index < waypoints.Count)
                Debug.DrawLine(mover.position + Vector3.up * 0.1f,
                               waypoints[index] + Vector3.up * 0.1f,
                               Color.yellow, 0f);
        }
    }

    public void Stop()
    {
        done = true;
        if (state != null)
        {
            state.HasPath = false; state.IsMoving = false;
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
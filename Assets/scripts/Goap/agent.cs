using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor;
    [SerializeField] Sensor attackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] Transform foodShack;
    [SerializeField] Transform doorOnePosition;
    [SerializeField] Transform doorTwoPosition;

    NavMeshAgent navMeshAgent;
    Rigidbody rb;

    [Header("Stats")]
    public float health = 100;
    public float stamina = 100;

    CountdownTimer statsTimer;

    GameObject target;
    Vector3 destination;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    GoapFactory gFactory;
    IGoapPlanner gPlanner;

    bool allPreconditionsMet = true;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        gFactory = UnityServiceLocator.ServiceLocator.Global.Get<GoapFactory>();
        gPlanner = gFactory.CreatePlanner();
    }
    
    void Start()
    {
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    void SetupBeliefs()
    {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("AgentHealthLow", () => health < 30);
        factory.AddBelief("AgentIsHealthy", () => health >= 50);
        factory.AddBelief("AgentStaminaLow", () => stamina < 10);
        factory.AddBelief("AgentIsRested", () => stamina >= 50);

        factory.AddLocationBelief("AgentAtDoorOne", 3f, doorOnePosition);
        factory.AddLocationBelief("AgentAtDoorTwo", 3f, doorTwoPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);
        factory.AddLocationBelief("AgentAtFoodShack", 3f, foodShack);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
        factory.AddBelief("AttackingPlayer", () => false);
    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());

        actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(navMeshAgent, 10))
            .AddEffect(beliefs["AgentMoving"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToEatingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodShack.position))
            .AddEffect(beliefs["AgentAtFoodShack"])
            .Build());

        actions.Add(new AgentAction.Builder("Eat")
            .WithStrategy(new IdleStrategy(5))
            .AddPrecondition(beliefs["AgentAtFoodShack"])
            .AddEffect(beliefs["AgentIsHealthy"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorOne")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorOnePosition.position))
            .AddEffect(beliefs["AgentAtDoorOne"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorTwo")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorTwoPosition.position))
            .AddEffect(beliefs["AgentAtDoorTwo"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorOneToRestArea")
            .WithCost(2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            .AddPrecondition(beliefs["AgentAtDoorOne"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorTwoRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position))
            .AddPrecondition(beliefs["AgentAtDoorTwo"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("Rest")
            .WithStrategy(new IdleStrategy(5))
            .AddPrecondition(beliefs["AgentAtRestingPosition"])
            .AddEffect(beliefs["AgentIsRested"])
            .Build());

        actions.Add(new AgentAction.Builder("ChasePlayer")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => beliefs["PlayerInChaseRange"].Location))
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            .AddEffect(beliefs["PlayerInAttackRange"])
            .Build());

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(2f))
            .AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Chill Out")
            .WithPriority(0)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build());

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentMoving"])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepHealthUp")
            .WithPriority(4)
            .WithDesiredEffect(beliefs["AgentIsHealthy"])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepStaminaUp")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["AgentIsRested"])
            .Build());

        goals.Add(new AgentGoal.Builder("SeekAndDestroy")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
            .Build());
    }

    void SetupTimers()
    {
        statsTimer = new CountdownTimer(2f);
        statsTimer.OnTimerStop += () => {
            UpdateStats();
            statsTimer.Start();
        };
        statsTimer.Start();
    }

    void UpdateStats()
    {
        stamina += InRangeOf(restingPosition.position, 3f) ? 20 : -10;
        health += InRangeOf(foodShack.position, 3f) ? 20 : -5;
        stamina = Mathf.Clamp(stamina, 0, 100);
        health = Mathf.Clamp(health, 0, 100);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged()
    {
        currentAction = null;
        currentGoal = null;
    }

    void Update()
    {
        statsTimer.Tick(Time.deltaTime);

        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                currentAction = actionPlan.Actions.Pop();

                bool allPreconditionsMet = true; 

                foreach (var b in currentAction.Preconditions)
                {
                    if (!b.Evaluate())
                    {
                        Debug.LogWarning($"[GOAP] Belief '{b.Name}' failed for action '{currentAction.Name}'");
                        allPreconditionsMet = false;
                    }
                }

                if (allPreconditionsMet)
                {
                    Debug.Log($"[GOAP] Starting action: {currentAction.Name}");
                    currentAction.Start();
                }
                else
                {
                    Debug.LogWarning($"[GOAP] Preconditions not met for: {currentAction.Name}. Aborting.");

                    currentAction = null;
                    currentGoal = null;
                }

            }
        }

        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0)
                {
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan()
    {Debug.Log("plan");
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        // If we have a current goal, we only want to check goals with higher priority
        if (currentGoal != null)
        {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionPlan = potentialPlan;
        }
        try
        {
            Debug.Log($"{priorityLevel} - {currentGoal.Name}");
        }
        catch
        {
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
public class GoapAgent : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    AnimationController animations;
    Rigidbody rb;

    CountDownTimer StatTimer;

    GameObject target;
    Vector3 destination;

    Goals LastGoal;
    public Goals CurrentGoals;
    public ActionPlan actionPlan;
    public AgentAction CurrentAction;

    public Dictionary<string, AIBeliefs> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<Goals> goals;

    [Header("Sensors")]
    [SerializeField] Sensor ChaseSensor;
    [SerializeField] Sensor AttackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform RestingPosition;
    [SerializeField] Transform FoodShack;
    [SerializeField] Transform DoorOnePosition;
    [SerializeField] Transform DoorTwoPosition;

    [Header("stats")]
    public float Health = 100;
    public float stamina = 100;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        SetupTimers();
        SetUpBeliefs();
        SetUpAtctions();
        SetUpGoals();
    }
    void SetUpBeliefs()
    {
        beliefs = new Dictionary<string, AIBeliefs>();
        BeliefsFactory factory = new BeliefsFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
    }

    void SetUpAtctions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"]).Build());
        actions.Add(new AgentAction.Builder("moving")
            .WithStrategy(new MovingStrategy(navMeshAgent, 10))
            .AddEffect(beliefs["AgentMoving"]).Build());
    }

    void SetUpGoals()
    {
        goals = new HashSet<Goals>();

        goals.Add(new Goals.Builder("Relax")
            .WithPrioraty(1)
            .WithDesieredEffect(beliefs["Nothing"]).Build());
        goals.Add(new Goals.Builder("move")
        .WithPrioraty(1)
        .WithDesieredEffect(beliefs["moving"]).Build());
    }
    void SetupTimers()
    {
        StatTimer = new CountDownTimer(2f);
        StatTimer.OnTimerStop += () =>
        {
            UpdateState();
            StatTimer.Start();
        };
        StatTimer.Start();
    }



    // TODO move to stats system
    void UpdateState()
    {
        stamina += InRangeOf(RestingPosition.position, 3f) ? 20 : -10;
        Health += InRangeOf(FoodShack.position, 3f) ? 20 : -5;
        stamina = Mathf.Clamp(stamina, 0, 100);
        Health = Mathf.Clamp(stamina, 0, 100);
    }
    bool InRangeOf(Vector3 pos, float range) =>
        Vector3.Distance(transform.position, pos) < range;
    void OnEnable() => ChaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => ChaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged()
    {
        Debug.Log("target changed");
        CurrentAction = null;
        CurrentGoals = null;
    }

}

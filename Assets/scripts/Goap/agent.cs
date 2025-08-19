using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] Transform woodShopCounter_Survivor;
    [SerializeField] Transform woodShopCounter_Worker;
    [SerializeField] Transform firePlace;
    [SerializeField] Transform forestPosition;

    

    [Header("Stats")]
    public float FireTimer = 10;
    public float health = 100;
    public float stamina = 100;
    public int woodCount = 0;
    public int fireplaceWood = 0;
    [SerializeField] float handoffRadius = 5f;

    CountdownTimer statsTimer;
    CountdownTimer firewoodBurnTimer;

    GameObject target;
    Vector3 destination;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    [SerializeField] GoapAgent survivorRef;
    NavMeshAgent navMeshAgent;
    Rigidbody rb;
    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    GoapFactory gFactory;
    IGoapPlanner gPlanner;

    public bool allPreconditionsMet = true;
    public bool FireInFirePlace = true;
    public enum AgentRole { Survivor, Woodworker }

    [Header("Role Settings")]
    [SerializeField] AgentRole role = AgentRole.Survivor;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        gFactory = UnityServiceLocator.ServiceLocator.Global.Get<GoapFactory>();
        gPlanner = gFactory.CreatePlanner();
        

        if (CompareTag("Survivor"))
        {
            role = AgentRole.Survivor;
        }
        
        if (CompareTag("Woodworker"))
        {
            role = AgentRole.Woodworker;
        }

        GameObject survivorObj = GameObject.FindGameObjectWithTag("Survivor");
        if (survivorObj != null)
        {
            survivorRef = survivorObj.GetComponent<GoapAgent>();
        }
        else
        {
            Debug.LogWarning("No Survivor found in the scene!");
        }
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

        if (role == AgentRole.Survivor)
        {
            factory.AddBelief("AgentHealthLow", () => health < 30);
            factory.AddBelief("AgentIsHealthy", () => health >= 50);
            factory.AddBelief("AgentStaminaLow", () => stamina < 10);
            factory.AddBelief("AgentIsRested", () => stamina >= 50);

            factory.AddBelief("HasAnyWood", () => woodCount >= 1);
            factory.AddBelief("HasAtLeast3Wood", () => woodCount >= 3);
            factory.AddBelief("FireplaceHasAtLeast3Wood", () => fireplaceWood >= 3);
            factory.AddBelief("NeedsWood", () => fireplaceWood <= 1);

            factory.AddLocationBelief("AgentAtDoorOne", 3f, doorOnePosition);
            factory.AddLocationBelief("AgentAtDoorTwo", 3f, doorTwoPosition);
            factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);
            factory.AddLocationBelief("AgentAtFoodShack", 3f, foodShack);
            factory.AddLocationBelief("AgentAtWoodShop", 5f, woodShopCounter_Survivor);
            factory.AddLocationBelief("AgentAtFireplace", 3f, firePlace);
        }
        if (role == AgentRole.Woodworker)
        {
            factory.AddBelief("HasAnyWood", () => woodCount > 0);
            factory.AddBelief("HasAtLeast3Wood", () => woodCount >= 3);

            factory.AddLocationBelief("AgentAtWoodShop", 5f, woodShopCounter_Worker);
            factory.AddLocationBelief("AgentAtForest", 3f, forestPosition);

            factory.AddBelief("SurvivorAtWoodShop",() => Vector3.Distance(survivorRef.transform.position, woodShopCounter_Survivor.position) < 5f);

            factory.AddBelief("SurvivorNeedsWood",() => survivorRef.woodCount < 3);

            factory.AddBelief("RequestActiveAtCounter",
                () => beliefs["SurvivorAtWoodShop"].Evaluate()
                && beliefs["SurvivorNeedsWood"].Evaluate());

            factory.AddBelief("SurvivorStocked3",
                () => survivorRef != null && survivorRef.woodCount >= 3);
        }
    }
    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(3))
            .AddEffect(beliefs["Nothing"])
            .Build());

        if (role == AgentRole.Survivor)
        {
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
           // -----------------------------------------WOOD-------------------------------------------------//
            
            actions.Add(new AgentAction.Builder("MoveToWoodShop")
                .WithStrategy(new MoveStrategy(navMeshAgent, () => woodShopCounter_Survivor.position))
                .AddPrecondition(beliefs["NeedsWood"])
                .AddEffect(beliefs["AgentAtWoodShop"])
                .Build());

            actions.Add(new AgentAction.Builder("WaitForWood")
                .WithStrategy(new RepeatCallbackUntilStrategy(2f,
                    () => woodCount >= 3,     
                    () => {}
                ))
                .AddPrecondition(beliefs["AgentAtWoodShop"])
                .AddPrecondition(beliefs["NeedsWood"])
                .AddEffect(beliefs["HasAtLeast3Wood"])
                .AddEffect(beliefs["HasAnyWood"])
                .Build());


            actions.Add(new AgentAction.Builder("MoveToFireplace")
                .WithStrategy(new MoveStrategy(navMeshAgent, () => firePlace.position))
                .AddPrecondition(beliefs["HasAnyWood"])
                .AddEffect(beliefs["AgentAtFireplace"])
                .Build());

            actions.Add(new AgentAction.Builder("DepositAllWood")
                .WithStrategy(new RepeatCallbackUntilStrategy(1f,
                    () => woodCount == 0,
                    () => { woodCount--; fireplaceWood++; }))
                .AddPrecondition(beliefs["AgentAtFireplace"])
                .AddPrecondition(beliefs["HasAnyWood"])
                .AddPrecondition(beliefs["HasAtLeast3Wood"])
                .AddEffect(beliefs["FireplaceHasAtLeast3Wood"])
                .Build());
        }

        if (role == AgentRole.Woodworker)
        {
            actions.Add(new AgentAction.Builder("ReturnToShop")
                .WithStrategy(new MoveStrategy(navMeshAgent, () => woodShopCounter_Worker.position))
                .AddEffect(beliefs["AgentAtWoodShop"])
                .Build());

            actions.Add(new AgentAction.Builder("IdleAtShop")
                .WithStrategy(new IdleStrategy(1))
                .AddPrecondition(beliefs["AgentAtWoodShop"])
                .AddEffect(beliefs["AgentIdle"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToForestForRequest")
                .WithStrategy(new MoveStrategy(navMeshAgent, () => forestPosition.position))
                .AddPrecondition(beliefs["SurvivorNeedsWood"])
                .AddPrecondition(beliefs["RequestActiveAtCounter"])
                .AddEffect(beliefs["AgentAtForest"])
                .Build());

            actions.Add(new AgentAction.Builder("ChopUntilThree")
                .WithStrategy(new RepeatCallbackUntilStrategy(1f,
                    () => woodCount >= 3,
                    () => woodCount = Mathf.Min(woodCount + 1, 4)))
                .AddPrecondition(beliefs["AgentAtForest"])
                .AddEffect(beliefs["HasAtLeast3Wood"])
                .AddEffect(beliefs["HasAnyWood"])
                .Build());

            actions.Add(new AgentAction.Builder("ReturnToCounterWithWood")
                .WithStrategy(new MoveStrategy(navMeshAgent, () => woodShopCounter_Worker.position))
                .AddPrecondition(beliefs["HasAtLeast3Wood"])
                .AddEffect(beliefs["AgentAtWoodShop"])
                .Build());

            actions.Add(new AgentAction.Builder("GiveWoodToSurvivor")
                .WithStrategy(new RepeatCallbackUntilStrategy(1f,
                    () => woodCount == 0,
                    () =>
                    {
                        if (survivorRef == null || woodCount <= 0) return;

                        bool survivorInPlace = Vector3.Distance(
                            survivorRef.transform.position, woodShopCounter_Survivor.position) < handoffRadius;

                        bool workerInPlace = Vector3.Distance(
                            transform.position, woodShopCounter_Worker.position) < handoffRadius;

                        bool closeToEachOther = Vector3.Distance(
                            transform.position, survivorRef.transform.position) < handoffRadius;

                        if ((survivorInPlace && workerInPlace) || closeToEachOther)
                        {
                            woodCount--;
                            survivorRef.woodCount = Mathf.Min(survivorRef.woodCount + 1, 3);
                        }
                    }))
                .AddPrecondition(beliefs["HasAnyWood"])
                .AddPrecondition(beliefs["AgentAtWoodShop"])
                .AddEffect(beliefs["SurvivorStocked3"])
                .Build());
        }
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Chill Out")
            .WithPriority(0)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build());

        if (role == AgentRole.Survivor)
        {
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

            goals.Add(new AgentGoal.Builder("StockFireplaceTWhenLow")
                .WithPriority(3)
                .WithDesiredEffect(beliefs["FireplaceHasAtLeast3Wood"])
                .Build());
        }

        if (role == AgentRole.Woodworker)
        {
            goals.Add(new AgentGoal.Builder("StayAtCounter")
                .WithPriority(1)
                .WithDesiredEffect(beliefs["AgentAtWoodShop"])
                .Build());

            goals.Add(new AgentGoal.Builder("FulfillSurvivorRequest")
                .WithPriority(3)
                .WithDesiredEffect(beliefs["SurvivorStocked3"])
                .Build());
        }
    }

    void SetupTimers()
    {
        if (role == AgentRole.Survivor)
        {
            //Debug.LogError($"{role}: starting timers");
            statsTimer = new CountdownTimer(2f);
            statsTimer.OnTimerStop += () =>
            {
                //Debug.LogError($"{role}: in update");
                UpdateStats();
                statsTimer.Start();
            };
            statsTimer.Start();

            firewoodBurnTimer = new CountdownTimer(FireTimer);
            firewoodBurnTimer.OnTimerStop += () =>
            {
                //Debug.LogError($"{role}: in firewood");
                UpdateFireWood();
                firewoodBurnTimer.Start();

            };
                firewoodBurnTimer.Start();
        }
    }

    void UpdateFireWood()
    {
        //Debug.LogError($"{role}: MY WOOD!");
        if (fireplaceWood >= 1)
        {
            fireplaceWood = Mathf.Max(0, fireplaceWood - 1);
        }
    }

    void UpdateStats()
    {
        //Debug.Log($"{role} updated stats");
        //stamina += InRangeOf(restingPosition.position, 3f) ? 20 : Random.Range(-1,-10);
        //health += InRangeOf(foodShack.position, 3f) ? 20 : -5;
        //stamina = Mathf.Clamp(stamina, 0, 100);
        //health = Mathf.Clamp(health, 0, 100);
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
        if(role == AgentRole.Survivor)
        {
            statsTimer.Tick(Time.deltaTime);
            if (fireplaceWood > 0)
            {
            firewoodBurnTimer.Tick( Time.deltaTime);
            }
        }
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
                        Debug.LogWarning($"[GOAP][{gameObject.tag}/{role}] '{b.Name}' failed for '{currentAction.Name}'");
                        allPreconditionsMet = false;
                    }
                }

                if (allPreconditionsMet)
                {
                    Debug.Log($"[GOAP] [{gameObject.tag}/{role}]Starting action: {currentAction.Name}");
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
    {
        Debug.Log("plan");
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

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
            Debug.Log($"[{gameObject.tag}/{role}]{priorityLevel} - {currentGoal.Name}");
        }
        catch
        {
        }
    }
    public void ReceiveWood(int amount = 1)
    {
        fireplaceWood = Mathf.Clamp(fireplaceWood + amount, 0, 3);
    }

}
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor;
    [SerializeField] Sensor attackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform woodShopCounter_Survivor;
    [SerializeField] Transform woodShopCounter_Worker;
    [SerializeField] Transform firePlace;
    [SerializeField] Transform forestPosition;
    [SerializeField] Transform butcherShop_Survivor;
    [SerializeField] Transform butcherShop_Worker;
    [SerializeField] Transform meatLocation;

    [Header("Movement")]
    [SerializeField] Transform mover;
    [SerializeField] float moveSpeed = 10f;
    MovementState movementState = new();
    Rigidbody rb;

    [Header("Stats")]
    public float FireTimer = 10;
    public float health = 100;
    public float stamina = 100;
    public int woodCount = 0;
    public int fireplaceWood = 0;
    public int RawMeat = 0;
    public int CookedMeat = 0;
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


    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    GoapFactory gFactory;
    IGoapPlanner gPlanner;

    public bool allPreconditionsMet = true;
    public bool FireInFirePlace = true;
    public enum AgentRole { Survivor, Woodworker, Butcher }

    [Header("Role Settings")]
    [SerializeField] AgentRole role = AgentRole.Survivor;

    [Header("Chat Bubble")]
    [SerializeField] GameObject chatBubblePrefab;
    GameObject chatBubbleInstance;
    TextMeshProUGUI chatBubbleText;
    [SerializeField] Vector3 bubbleOffset = new Vector3(0, 2f, 0);
    [SerializeField] float bubbleDuration = 2f;
    float bubbleTimer = 0f;
    private string lastActionShown = "";

    void Awake()
    {

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (mover == null)
        {
            mover = transform;
        }
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
        if (chatBubblePrefab != null)
        {
            chatBubbleInstance = Instantiate(chatBubblePrefab, transform.position + bubbleOffset, Quaternion.identity, transform);
            chatBubbleText = chatBubbleInstance.GetComponentInChildren<TextMeshProUGUI>();
            chatBubbleInstance.SetActive(false);
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
        factory.AddBelief("AgentIdle", () => !movementState.HasPath);
        factory.AddBelief("AgentMoving", () => movementState.HasPath);

        if (role == AgentRole.Survivor)
        {   //----------------------------stats--------------------------------------------
            factory.AddBelief("AgentHealthLow", () => health <= 30);
            factory.AddBelief("AgentIsHealthy", () => health >= 60);

            factory.AddBelief("AgentStaminaLow", () => stamina < 10);
            factory.AddBelief("AgentOutofStamina", () => stamina <= 1);
            factory.AddBelief("AgentIsRested", () => stamina >= 50);
            //----------------------------wood--------------------------------------------
            factory.AddBelief("HasAnyWood", () => woodCount >= 1);
            factory.AddBelief("HasAtLeast3Wood", () => woodCount >= 3);
            factory.AddBelief("FireplaceHasAtLeast3Wood", () => fireplaceWood >= 3);
            factory.AddBelief("FirePlaceHasWood", () => fireplaceWood >= 1);
            factory.AddBelief("NeedsWood", () => fireplaceWood <= 1);
            factory.AddLocationBelief("AgentAtWoodShop", 5f, woodShopCounter_Survivor);
            //----------------------------fireplace--------------------------------------------
            factory.AddLocationBelief("AgentAtFireplace", 3f, firePlace);
            //----------------------------butcher--------------------------------------------
            factory.AddBelief("HasRawMeat", () => RawMeat >= 1);
            factory.AddBelief("HasCookedMeat", () => CookedMeat >= 1);
            factory.AddLocationBelief("AgentAtButcher", 5f, butcherShop_Survivor);
        }
        if (role == AgentRole.Woodworker)
        {
            factory.AddBelief("HasAnyWood", () => woodCount > 0);
            factory.AddBelief("HasAtLeast3Wood", () => woodCount >= 3);

            factory.AddLocationBelief("AgentAtWoodShop", 5f, woodShopCounter_Worker);
            factory.AddLocationBelief("AgentAtForest", 3f, forestPosition);

            factory.AddBelief("SurvivorAtWoodShop", () => Vector3.Distance(survivorRef.transform.position, woodShopCounter_Survivor.position) < 5f);

            factory.AddBelief("SurvivorNeedsWood", () => survivorRef.woodCount < 3);

            factory.AddBelief("RequestActiveAtCounter",
                () => beliefs["SurvivorAtWoodShop"].Evaluate()
                && beliefs["SurvivorNeedsWood"].Evaluate());

            factory.AddBelief("SurvivorStocked3",
                () => survivorRef != null && survivorRef.woodCount >= 3);
        }
        if (role == AgentRole.Butcher)
        {
            factory.AddBelief("HasRawMeat", () => RawMeat >= 1);

            factory.AddLocationBelief("AgentAtButcherShop", 5f, butcherShop_Worker);
            factory.AddLocationBelief("AgentAtMeatLocation", 3f, meatLocation);

            factory.AddBelief("SurvivorAtButcherShop",
                () => Vector3.Distance(survivorRef.transform.position, butcherShop_Survivor.position) < 5f);

            factory.AddBelief("SurvivorNeedsMeat", () => survivorRef.RawMeat <= 0);

            factory.AddBelief("RequestActiveAtButcherCounter",
                () => beliefs["SurvivorAtButcherShop"].Evaluate()
                && beliefs["SurvivorNeedsMeat"].Evaluate());

            factory.AddBelief("SurvivorStockedMeat",
                () => survivorRef != null && survivorRef.RawMeat >= 1);
        }

    }
    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(3))
            .AddEffect(beliefs["AgentIdle"])
            .Build());
        if (role == AgentRole.Survivor)
        {

            actions.Add(new AgentAction.Builder("Rest")
                .WithStrategy(new TimedCallbackStrategy(3f,
                () => health = Mathf.Clamp(health = 100, 0, 100)))
                .AddPrecondition(beliefs["AgentIdle"])
                .AddEffect(beliefs["AgentIsRested"])
                .Build());
            // -----------------------------------------WOOD-------------------------------------------------//

            actions.Add(new AgentAction.Builder("MoveToWoodShop")
                .WithStrategy(new MoveStrategy(mover, () => woodShopCounter_Survivor.position, movementState, moveSpeed))
                .AddPrecondition(beliefs["NeedsWood"])
                .AddEffect(beliefs["AgentAtWoodShop"])
                .Build());

            actions.Add(new AgentAction.Builder("WaitForWood")
                .WithStrategy(new RepeatCallbackUntilStrategy(2f,
                    () => woodCount >= 3,
                    () => { }
                ))
                .AddPrecondition(beliefs["AgentAtWoodShop"])
                .AddPrecondition(beliefs["NeedsWood"])
                .AddEffect(beliefs["HasAtLeast3Wood"])
                .AddEffect(beliefs["HasAnyWood"])
                .Build());


            actions.Add(new AgentAction.Builder("MoveToFireplace")
                .WithStrategy(new MoveStrategy(mover, () => firePlace.position, movementState, moveSpeed))
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
                .AddEffect(beliefs["FirePlaceHasWood"])
                .Build());
            // -----------------------------------------BUTCHER-------------------------------------------------//
            actions.Add(new AgentAction.Builder("MoveToButcher")
            .WithStrategy(new MoveStrategy(mover, () => butcherShop_Survivor.position, movementState, moveSpeed))
            .AddPrecondition(beliefs["AgentHealthLow"])
            .AddEffect(beliefs["AgentAtButcher"])
            .Build());

            actions.Add(new AgentAction.Builder("WaitForMeat")
                .WithStrategy(new RepeatCallbackUntilStrategy(2f,
                    () => RawMeat >= 1,
                    () => { }
                ))
                .AddPrecondition(beliefs["AgentAtButcher"])
                .AddPrecondition(beliefs["AgentHealthLow"])
                .AddEffect(beliefs["HasRawMeat"])
                .Build());

            actions.Add(new AgentAction.Builder("MoveToFireplace")
                .WithStrategy(new MoveStrategy(mover, () => firePlace.position, movementState, moveSpeed))
                .AddPrecondition(beliefs["HasRawMeat"])
                .AddEffect(beliefs["AgentAtFireplace"])
                .Build());

            actions.Add(new AgentAction.Builder("CookRawMeat")
                .WithStrategy(new TimedCallbackStrategy(4f,
                () =>
                {
                    RawMeat--;
                    CookedMeat++;
                }))
                .AddPrecondition(beliefs["HasRawMeat"])
                .AddPrecondition(beliefs["FirePlaceHasWood"])
                .AddPrecondition(beliefs["AgentAtFireplace"])
                .AddEffect(beliefs["HasCookedMeat"])
                .Build());

            actions.Add(new AgentAction.Builder("Eat")
                .WithStrategy(new TimedCallbackStrategy(4f,
                () =>
                {
                    CookedMeat--;
                    health = 100;
                }))
                .AddPrecondition(beliefs["HasCookedMeat"])
                .AddEffect(beliefs["AgentIsHealthy"])
                .Build());


        }

        if (role == AgentRole.Woodworker)
        {
            actions.Add(new AgentAction.Builder("ReturnToShop")
                .WithStrategy(new MoveStrategy(mover, () => woodShopCounter_Worker.position, movementState, moveSpeed))
                .AddEffect(beliefs["AgentAtWoodShop"])
                .Build());

            actions.Add(new AgentAction.Builder("IdleAtShop")
                .WithStrategy(new IdleStrategy(1))
                .AddPrecondition(beliefs["AgentAtWoodShop"])
                .AddEffect(beliefs["AgentIdle"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToForestForRequest")
                .WithStrategy(new MoveStrategy(mover, () => forestPosition.position, movementState, moveSpeed))
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
                .WithStrategy(new MoveStrategy(mover, () => woodShopCounter_Worker.position, movementState, moveSpeed))
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

        if (role == AgentRole.Butcher)
        {
            actions.Add(new AgentAction.Builder("ReturnToButcherShop")
                .WithStrategy(new MoveStrategy(mover, () => butcherShop_Worker.position, movementState, moveSpeed))
                .AddEffect(beliefs["AgentAtButcherShop"])
                .Build());

            actions.Add(new AgentAction.Builder("IdleAtButcherShop")
                .WithStrategy(new IdleStrategy(1))
                .AddPrecondition(beliefs["AgentAtButcherShop"])
                .AddEffect(beliefs["AgentIdle"])
                .Build());

            actions.Add(new AgentAction.Builder("GoToMeatLocationForRequest")
                .WithStrategy(new MoveStrategy(mover, () => meatLocation.position, movementState, moveSpeed))
                .AddPrecondition(beliefs["SurvivorNeedsMeat"])
                .AddPrecondition(beliefs["RequestActiveAtButcherCounter"])
                .AddEffect(beliefs["AgentAtMeatLocation"])
                .Build());

            actions.Add(new AgentAction.Builder("CollectMeatUntilThree")
               .WithStrategy(new TimedCallbackStrategy(3f,
                () => RawMeat++))
                .AddPrecondition(beliefs["AgentAtMeatLocation"])
                .AddEffect(beliefs["HasRawMeat"])
                .Build());

            actions.Add(new AgentAction.Builder("ReturnToButcherCounterWithMeat")
                .WithStrategy(new MoveStrategy(mover, () => butcherShop_Worker.position, movementState, moveSpeed))
                .AddPrecondition(beliefs["HasRawMeat"])
                .AddEffect(beliefs["AgentAtButcherShop"])
                .Build());

            actions.Add(new AgentAction.Builder("GiveMeatToSurvivor")
                .WithStrategy(new TimedCallbackStrategy(2f,
                () =>
                {
                    RawMeat--;
                    survivorRef.RawMeat++;
                    })
                {

                })
                .AddPrecondition(beliefs["HasRawMeat"])
                .AddPrecondition(beliefs["AgentAtButcherShop"])
                .AddEffect(beliefs["SurvivorStockedMeat"])
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
            goals.Add(new AgentGoal.Builder("KeepHealthUp")
                .WithPriority(3)
                .WithDesiredEffect(beliefs["AgentIsHealthy"])
                .Build());

            goals.Add(new AgentGoal.Builder("KeepStaminaUp")
                .WithPriority(4)
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

        if (role == AgentRole.Butcher)
        {
            goals.Add(new AgentGoal.Builder("StayAtButcherCounter")
                .WithPriority(1)
                .WithDesiredEffect(beliefs["AgentAtButcherShop"])
                .Build());


            goals.Add(new AgentGoal.Builder("FulfillSurvivorMeatRequest")
                .WithPriority(3)
                .WithDesiredEffect(beliefs["SurvivorStockedMeat"])
                .Build());
        }
    }

    void SetupTimers()
    {
        if (role == AgentRole.Survivor)
        {
            //Debug.LogError($"{role}: starting timers");
            statsTimer = new CountdownTimer(1f);
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


        if (role == AgentRole.Survivor)
        {
            stamina += !movementState.HasPath ? 5f : (movementState.HasPath ? -Random.Range(1, 5) : 0);
            stamina = Mathf.Clamp(stamina, 0, 100);

            health -= 1f;
            health = Mathf.Clamp(health, 0, 100);
        }
    }


    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged()
    {
        movementState.HasPath = false;
        movementState.IsMoving = false;

        currentAction = null;
        currentGoal = null;
    }

    void Update()
    {
        if (role == AgentRole.Survivor)
        {
            statsTimer.Tick(Time.deltaTime);
            if (fireplaceWood > 0)
            {
                firewoodBurnTimer.Tick(Time.deltaTime);
            }
        }
        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                movementState.HasPath = false;
                movementState.IsMoving = false;

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

                    // --- CHAT BUBBLE DISPLAY ---
                    if (currentAction.Name != lastActionShown)
                    {
                        ShowChatBubble(currentAction.Name);
                        lastActionShown = currentAction.Name;
                    }
                }
                else
                {
                    Debug.LogWarning($"[GOAP] Preconditions not met for: {currentAction.Name}. Aborting.");
                    movementState.HasPath = false;
                    movementState.IsMoving = false;

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
        // --- CHAT BUBBLE TIMER ---
        if (chatBubbleInstance != null)
        {
            // Make the bubble follow the agent
            chatBubbleInstance.transform.position = transform.position + bubbleOffset;

            // Update bubble text if currentAction changed
            if (currentAction != null && currentAction.Name != lastActionShown)
            {
                ShowChatBubble(currentAction.Name);
                lastActionShown = currentAction.Name;
            }

            // Countdown timer for hiding bubble
            if (chatBubbleInstance.activeSelf)
            {
                bubbleTimer -= Time.deltaTime;
                if (bubbleTimer <= 0f)
                {
                    chatBubbleInstance.SetActive(false);
                    lastActionShown = ""; // Reset for next action
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
    public void ReceiveMeat(int amount = 1)
    {
        RawMeat = Mathf.Clamp(RawMeat + amount, 0, 3);
    }
    void ShowChatBubble(string message)
    {
        if (chatBubbleInstance != null)
        {
            chatBubbleInstance.SetActive(true);
            chatBubbleText.text = message;
            bubbleTimer = bubbleDuration;
        }
    }



}

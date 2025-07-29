using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface GoapPlannerI
{
    ActionPlan Plan(GoapAgent agent, HashSet<Goals> goals, Goals mostRecentGoal = null);
}
public class GoapPlanner : GoapPlannerI
{
    public ActionPlan Plan(GoapAgent agent, HashSet<Goals> goals, Goals mostRecentGoal = null)
    {
     List<Goals> OrderedGoals = goals
    .Where(g => g.DesiredEffect.Any(b => !b.Evaluate()))
    .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01f : g.Priority)
    .ToList();

        foreach (var goal in OrderedGoals)
        {
            Node NoalNode = new Node(null, null, goal.DesiredEffect, 0);

            if (FindPath(GoalNode, agent.actions))
            {

            }
        }
    } 

    bool FindPath(Node parent, HashSet<AgentAction> actions)
    {
        foreach (var action in actions)
        var requiredEffect = parent.RequiredEffect;
    }
}

public class Node
{
    public Node Parent { get; }
    public AgentAction Action { get; }
    public HashSet<AIBeliefs> RequiredEffect { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public Node(Node parent, AgentAction action, HashSet<AIBeliefs> effects, float cost)
    {
        Parent = parent;
        Action = action;
        RequiredEffect = new HashSet<AIBeliefs>(effects);
        Leaves = new List<Node>(); ;
        Cost = cost;
    }
}
public class ActionPlan
{
    public Goals AgentGoal {  get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlan(Goals goals, Stack<AgentAction> actions, float totalCost)
    {
        AgentGoal = goals;
        Actions = actions;
        TotalCost = totalCost;
    }
}

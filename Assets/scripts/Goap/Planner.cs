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
            Node GoalNode = new Node(null, null, goal.DesiredEffect, 0);

            if (FindPath(GoalNode, agent.actions))
            {
                if (GoalNode.IsLeafDead) continue;

                Stack<AgentAction> actoinStack = new Stack<AgentAction>();
                while (GoalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = GoalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    GoalNode = cheapestLeaf;
                    actoinStack.Push(cheapestLeaf.Action);
                }

                return new ActionPlan(goal, actoinStack,GoalNode.Cost);
            }
        }
        Debug.LogWarning("no plan found");
        return null;
    }

    bool FindPath(Node parent, HashSet<AgentAction> actions)
    {
        var orderedActions = actions.OrderBy(a => a.Cost);

        foreach (var action in orderedActions)
        {
            var requiredEffects = parent.RequiredEffects;

            requiredEffects.RemoveWhere(b => b.Evaluate());

            if (requiredEffects.Count == 0)
            {
                return true;
            }

            if (action.Effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<AIBeliefs>(requiredEffects);
                newRequiredEffects.ExceptWith(action.Effects);
                newRequiredEffects.UnionWith(action.Preconditions);

                var newAvailableAction = new HashSet<AgentAction>(actions);
                newAvailableAction.Remove(action);

                var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                if (FindPath(newNode, newAvailableAction))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                }
                if (newRequiredEffects.Count == 0) return true;
            }
        }
        return false;
    }
}

public class Node
{
    public Node Parent { get; }
    public AgentAction Action { get; }
    public HashSet<AIBeliefs> RequiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public Node(Node parent, AgentAction action, HashSet<AIBeliefs> effects, float cost)
    {
        Parent = parent;
        Action = action;
        RequiredEffects = new HashSet<AIBeliefs>(effects);
        Leaves = new List<Node>(); ;
        Cost = cost;
    }
}
public class ActionPlan
{
    public Goals AgentGoal { get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlan(Goals goals, Stack<AgentAction> actions, float totalCost)
    {
        AgentGoal = goals;
        Actions = actions;
        TotalCost = totalCost;
    }
}

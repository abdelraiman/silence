using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAction
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AIBeliefs> Preconditions { get; } = new();
    public HashSet<AIBeliefs> Effects { get; } = new();

    ActionStratagy Stratagy;
    public bool Complete => Stratagy.Complete;
    AgentAction(string name)
    {
        Name = name;
    }
    public void Start() => Stratagy.Start();

    public void update(float deltatime)
    {
        if (Stratagy.Complete)
        {
            Stratagy.Update(deltatime);
        }

        if (!Stratagy.Complete) return;

        foreach (var effect  in Effects)
        {
            effect.Evaluate();
        }
    }

    public void Stop() => Stratagy.Stop();

    public class Builder
    {
        readonly AgentAction action;

        public Builder(string name)
        {
            action = new AgentAction(name)
            {
                Cost = 1
            };
        }

        public Builder WithCost(float cost)
        {
            action.Cost = cost;
            return this;
        }
        public Builder WithStrategy(ActionStratagy strategy)
        {
            action.Stratagy = strategy;
            return this;
        }

        public Builder AddPrecondition(AIBeliefs precondition)
        {
            action.Preconditions.Add(precondition);
            return this;
        }
        public Builder AddEffect(AIBeliefs effect)
        {
            action.Effects.Add(effect);
            return this;
        }
        public AgentAction Build()
        {
            return action;
        }
    }
}

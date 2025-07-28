using System.Collections.Generic;
using UnityEngine;

public class Goals : MonoBehaviour
{
    public string Name { get; }
    public float Priority { get; private set; }
    public HashSet<AIBeliefs> DesiredEffect { get; } = new();

    Goals(string name) 
    {
        Name = name;
    }

    public class Builder
    {
        readonly Goals goal;

        public Builder(string name)
        {
            goal = new Goals(name);
        }

        public Builder WithPrioraty(float priority)
        {
            goal.Priority = priority;
            return this;
        }

        public Builder WithDesieredEffect(AIBeliefs effects)
        {
            goal.DesiredEffect.Add(effects);
            return this;
        }

        public Goals Build()
        {
            return goal;
        }
    }
}

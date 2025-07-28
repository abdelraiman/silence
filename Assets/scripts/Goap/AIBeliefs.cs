using System;
using UnityEngine;

public class AIBeliefs
{
    public string Name { get; }

    public Func<bool> condition = () => false;
    public Func<Vector3> observedLocation = () => Vector3.zero;

    public Vector3 Location => observedLocation();

    public AIBeliefs(string name)
    {
        Name = name;
    }

    public bool Evaluate() => condition();
    public class Builder
    {
        readonly AIBeliefs belief;
        public Builder(string name)
        {
            belief = new AIBeliefs(name);
        }
        public Builder WithCondition(Func<bool> condition)
        {
            belief.condition = condition;
            return this;
        }
        public Builder WithCondition(Func<Vector3> observedLocation)
        {
            belief.observedLocation = observedLocation;
            return this; 
        }

        public AIBeliefs Build()
        {
            return belief;
        }
    }


}

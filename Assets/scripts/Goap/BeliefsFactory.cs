using System;
using System.Collections.Generic;
using UnityEngine;

public class BeliefsFactory
{
    readonly GoapAgent agent;
    readonly Dictionary<string, AIBeliefs> beliefs;

    public BeliefsFactory(GoapAgent agent,
        Dictionary<string, AIBeliefs> beliefs)
    {
        this.agent = agent;
        this.beliefs = beliefs;
    }

    public void AddBelief(String key, Func<bool> condition)
    {
        beliefs.Add(key, new AIBeliefs.Builder(key).WithCondition(condition).Build());
    }

    public void AddSensorBelief(string key, Sensor sensor)
    {
        beliefs.Add(key, new AIBeliefs.Builder(key).WithCondition(( ) => sensor.IsTargetInRange)
            .WithCondition(() => sensor.TargetPosition).Build());
    }
    public void AddLocationBeliefs(string key, float distance, Transform locationCondition) 
    {
        AddLocationBelief(key, distance, locationCondition.position);
    }
    public void AddLocationBelief(string key, float distance, Vector3 locationCondition)
    {
        beliefs.Add(key, new AIBeliefs.Builder(key)
            .WithCondition(() => inRangOf(locationCondition,distance))
            .WithCondition(() => locationCondition)
            .Build());
    }
    bool inRangOf(Vector3 pos, float range) =>
            Vector3.Distance(agent.transform.position, pos) < range;
}

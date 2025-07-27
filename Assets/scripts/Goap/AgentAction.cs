using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAction
{
    public string Name { get;}
    public float Cost { get; private set; }

    public HashSet<AIBeliefs> Preconditions { get; } = new();
    public HashSet<AIBeliefs> Effects { get; } = new();

    IActionStratagy Stratagy;
}

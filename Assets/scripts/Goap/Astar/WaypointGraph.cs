using System.Collections.Generic;
using UnityEngine;

public class WaypointGraph : MonoBehaviour
{
    public static WaypointGraph Instance { get; private set; }
    public List<WaypointNode> nodes = new List<WaypointNode>();

    void Awake() => Instance = this;

    [ContextMenu("Rebuild Links")]
    public void RebuildLinks()
    {
        nodes.Clear();
        nodes.AddRange(FindObjectsOfType<WaypointNode>());

        foreach (var n in nodes)
            n.neighbors.Clear();

        foreach (var a in nodes)
        {
            foreach (var b in nodes)
            {
                if (a == b) continue;
                float dist = Vector3.Distance(a.transform.position, b.transform.position);
                float maxR = Mathf.Min(a.autoLinkRadius, b.autoLinkRadius);
                if (dist > maxR) continue;

                Vector3 from = a.transform.position + Vector3.up * 0.2f;
                Vector3 to = b.transform.position + Vector3.up * 0.2f;

                if (!Physics.Linecast(from, to, a.obstacleMask))
                {
                    if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
                    if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
                }
            }
        }
    }


    public WaypointNode FindClosest(Vector3 pos)
    {
        if (nodes == null || nodes.Count == 0) return null;

        WaypointNode best = null;
        float bestSqr = float.PositiveInfinity;

        foreach (var n in nodes)
        {
            if (!n) continue;
            float d2 = (n.transform.position - pos).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = n;
            }
        }
        return best;
    }

}

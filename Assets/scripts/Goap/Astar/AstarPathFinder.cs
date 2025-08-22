using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AStarPathfinder
{
    class Info
    {
        public WaypointNode node;
        public Info parent;
        public float g, f;
    }

    public static bool FindPath(Vector3 startPos, Vector3 endPos, out List<Vector3> result)
    {
        result = null;

        var graph = WaypointGraph.Instance;
        if (graph == null || graph.nodes == null || graph.nodes.Count == 0)
            return false;

        int obstacleMask = LayerMask.GetMask("Obstical");
        if (!Physics.Linecast(startPos + Vector3.up * 0.2f, endPos + Vector3.up * 0.2f, obstacleMask))
        {
            result = new List<Vector3> { startPos, endPos };
            return true;
        }
        var startVisible = new List<WaypointNode>();
        var endVisible = new List<WaypointNode>();

        foreach (var n in graph.nodes)
        {
            if (!n) continue;
            Vector3 np = n.transform.position + Vector3.up * 0.2f;

            if (!Physics.Linecast(startPos + Vector3.up * 0.2f, np, obstacleMask))
                startVisible.Add(n);

            if (!Physics.Linecast(endPos + Vector3.up * 0.2f, np, obstacleMask))
                endVisible.Add(n);
        }

        const int k = 3;
        var startSeeds = (startVisible.Count > 0
            ? startVisible.OrderBy(n => (n.transform.position - startPos).sqrMagnitude)
            : graph.nodes.OrderBy(n => (n.transform.position - startPos).sqrMagnitude)).Take(k).ToList();

        var endTargetsList = (endVisible.Count > 0
            ? endVisible.OrderBy(n => (n.transform.position - endPos).sqrMagnitude)
            : graph.nodes.OrderBy(n => (n.transform.position - endPos).sqrMagnitude)).Take(k).ToList();

        if (startSeeds.Count == 0 || endTargetsList.Count == 0) return false;

        var endTargets = new HashSet<WaypointNode>(endTargetsList);

        float HeuToAnyEnd(WaypointNode a)
        {
            float best = float.PositiveInfinity;
            Vector3 ap = a.transform.position;
            foreach (var t in endTargets)
            {
                float d = (t.transform.position - ap).sqrMagnitude;
                if (d < best) best = d;
            }
            return Mathf.Sqrt(best);
        }

        var open = new List<Info>();
        var closed = new HashSet<WaypointNode>();
        var recMap = new Dictionary<WaypointNode, Info>();

        foreach (var s in startSeeds)
        {
            float g0 = Vector3.Distance(startPos, s.transform.position);
            var r = new Info { node = s, parent = null, g = g0, f = g0 + HeuToAnyEnd(s) };
            open.Add(r);
            recMap[s] = r;
        }

        Info goalReached = null;

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.f.CompareTo(b.f));
            var current = open[0];
            open.RemoveAt(0);

            if (endTargets.Contains(current.node))
            {
                goalReached = current;
                break;
            }

            closed.Add(current.node);

            foreach (var neighbor in current.node.neighbors)
            {
                if (!neighbor || closed.Contains(neighbor)) continue;

                float step = Vector3.Distance(current.node.transform.position, neighbor.transform.position);
                float tentativeG = current.g + step;

                if (!recMap.TryGetValue(neighbor, out var neighborRec))
                {
                    neighborRec = new Info { node = neighbor };
                    recMap[neighbor] = neighborRec;
                }

                if (!open.Contains(neighborRec) || tentativeG < neighborRec.g)
                {
                    neighborRec.parent = current;
                    neighborRec.g = tentativeG;
                    neighborRec.f = tentativeG + HeuToAnyEnd(neighbor);
                    if (!open.Contains(neighborRec)) open.Add(neighborRec);
                }
            }
        }

        if (goalReached == null) return false;

        var path = new List<Vector3>();
        var it = goalReached;
        while (it != null)
        {
            path.Add(it.node.transform.position);
            it = it.parent;
        }
        path.Reverse();
        path.Insert(0, startPos);
        path.Add(endPos);

        result = path;
        return true;
    }


        static float Heu(WaypointNode a, WaypointNode b) =>
        Vector3.Distance(a.transform.position, b.transform.position);
}

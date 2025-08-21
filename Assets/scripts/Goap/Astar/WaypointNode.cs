using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaypointNode : MonoBehaviour
{
    [Range(0.1f, 200f)] public float autoLinkRadius = 20f;
    [Tooltip("Physics layers considered obstacles blocking links")]
    public LayerMask obstacleMask = ~0;

    public List<WaypointNode> neighbors = new List<WaypointNode>();

    void OnValidate() { if (autoLinkRadius < 0.1f) autoLinkRadius = 0.1f; }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(transform.position, autoLinkRadius);

        Gizmos.color = Color.yellow;
        foreach (var n in neighbors)
            if (n) Gizmos.DrawLine(transform.position, n.transform.position);
    }
#endif
}

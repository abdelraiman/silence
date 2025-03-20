using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class takedownscript : MonoBehaviour
{
    [Header("takedown")]
    public bool takedown;
    public LayerMask Enemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        takedown = Physics.Raycast(transform.position, Vector3.forward, 1f, Enemy);
    }
}

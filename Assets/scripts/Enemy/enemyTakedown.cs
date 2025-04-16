using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyTakedown : MonoBehaviour
{
    public Animator anim;
    

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void tookdown()
    {
        anim.SetTrigger("TakedownTrigger");
        Debug.Log("dying");
    }
}

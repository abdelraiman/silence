using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyTakedown : MonoBehaviour
{
    public Animator anim;
    public bool dead = false;
    public GameObject GameObject;
    public int delaytime = 10;
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        GameObject = gameObject;
    }

    // Update is called once per frame
    public void tookdown()
    {
        dead = true;
        anim.SetTrigger("TakedownTrigger");
        Debug.Log("dying");
    }

    private void Update()
    {
        if (dead == true)
        {
            StartCoroutine(destroyenemy());
        }
    }

    IEnumerator destroyenemy()
    {
        yield return new WaitForSeconds(delaytime);
        Destroy(GameObject);
    }
}

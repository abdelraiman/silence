using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coroutines : MonoBehaviour
{
    [Header("coroutines")]
    public MonoBehaviour scriptToActivate;
    public float delayTime = 3f;
    // Start is called before the first frame update
    void Start()
    {
        scriptToActivate.enabled = false;
        StartCoroutine(ActivateScriptAfterDelay());
    }

    IEnumerator ActivateScriptAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        scriptToActivate.enabled = true;
    }
}

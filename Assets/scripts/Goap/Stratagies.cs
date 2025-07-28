using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionStratagy
{
    bool CanPerform {  get; }
    bool Complete { get; }

    void Start()
    {

    }

    void Update(float deltatime)
    {

    }

    void Stop() 
    {
        
    }
}

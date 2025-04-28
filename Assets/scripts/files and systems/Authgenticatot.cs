using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
public class Authgenticatot : MonoBehaviour
{
    public Text logTxt;
    

    async void Awake()
    {
        await UnityServices.InitializeAsync();
        await signInAnonymous();
    }

    public async void SignIn()
    {
        await signInAnonymous();
    }

    async Task signInAnonymous()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            print("sign in succes");
            print("Player Id:" + AuthenticationService.Instance.PlayerId);
            logTxt.text = "player id:" + AuthenticationService.Instance.PlayerId;
        }
        catch (AuthenticationException ex)
        {
            print("sign in failes");
            Debug.LogException(ex);
        }
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using UnityEngine.UI;
using Unity.Services.Core;
public class CloudSave : MonoBehaviour
{
    public Text status;
    public InputField inpf;

    public async void Start()
    {
        await UnityServices.InitializeAsync();
    }

     public async void SavedData()
    {
        var data = new Dictionary<string, object> { { "firstData", inpf.text } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("saved");    }
}

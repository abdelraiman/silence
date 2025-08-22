using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using UnityEngine.UI;
using Unity.Services.Core;
using System.Threading;
public class CloudSave : MonoBehaviour
{
    public bool Achivmerntyay;
    public InputField inpf;
    public int jump;
    float timer;
    int EnemyesKilled;
    public async void Start()
    {
        Achivmerntyay = true;
        await UnityServices.InitializeAsync();
    }
    public void Update()
    {
        Timer();
        if (timer > 4.9999 && jump > 4 && EnemyesKilled > 0 && !Achivmerntyay)
        {
            Achivmerntyay = true;
            Debug.Log("Achivmernt UNLOCKEDD!!!!!");
        }
    }
    public async void SavedData()
    {
        var data4 = new Dictionary<string, object> { { "Timer", timer } };
        var data3 = new Dictionary<string, object> { { "Elims", EnemyesKilled } };
        var data2 = new Dictionary<string, object> { { "Jump", jump } };
        if (inpf)
        {
            var data1 = new Dictionary<string, object> { { "Text", inpf.text } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data1);
        }


        await CloudSaveService.Instance.Data.Player.SaveAsync(data2);
        await CloudSaveService.Instance.Data.Player.SaveAsync(data3);
        await CloudSaveService.Instance.Data.Player.SaveAsync(data4);
        Debug.Log("saved");
    }
    public async void LoadData()
    {
        if (inpf)
        {
            Dictionary<string, string> serverData1 = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Text" });
            if (serverData1.ContainsKey("Text"))
            {
                inpf.text = serverData1["Text"];
                Debug.Log(serverData1["Text"]);
            }
        }

        Dictionary<string, string> serverData2 = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Jump" });
        Dictionary<string, string> serverData3 = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Elims" });
        Dictionary<string, string> serverData4 = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Timer" });

        if (!serverData2.ContainsKey("Jump") &&
            !serverData3.ContainsKey("Elims") && !serverData4.ContainsKey("Timer"))
        {
            Debug.Log("no data");
            return;
        }

        jump = int.Parse(serverData2["Jump"]);
        //Debug.Log(serverData2["Jump"]);
        Debug.Log("jumed" + jump);

        EnemyesKilled = int.Parse(serverData3["Elims"]);
        //Debug.Log(serverData3["Elims"]);
        Debug.Log("killed" + EnemyesKilled);

        timer = float.Parse(serverData4["Timer"]);
        //Debug.Log(serverData4["Timer"]);
        Debug.Log("time" + timer);
    }
    public void addjump()
    {
        jump++;
        //Debug.Log(jump);
    }

    public void addDeath()
    {
        EnemyesKilled++;
        //Debug.Log(jump);
    }

    public void Timer()
    {
        timer += Time.deltaTime;
        //Debug.Log(timer);
    }

    public void ResetData()
    {
        timer = 0;
        EnemyesKilled = 0;
        jump = 0;
        SavedData();
        Achivmerntyay = false;
        Debug.Log("Achivmernt no:(");
    }
}

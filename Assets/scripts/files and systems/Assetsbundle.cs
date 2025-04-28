using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Assetsbundle : MonoBehaviour
{
    string folderPath = "AssetBundles";
    string fileName = "stuff";
    string combinedPath;
    [Header("bools")]
    public bool box = false;
    public bool wall = false;

    private AssetBundle stuff; 

    // Start is called before the first frame update
    void Awake()
    {
        LoadAssetsBunndle();
        Loadprefab();
        Loadboxes();
    }

    void Loadprefab()
    {
        if (wall != true)
            return;
        Debug.Log("starting load"); 

        if (stuff == null)
        {
            Debug.Log("not fabbing");
            return; 
        }

        Debug.Log("fabbing");

        GameObject Wallprefab = stuff.LoadAsset<GameObject>("Wall");

        if (Wallprefab != null)
        {
          Instantiate(Wallprefab, Vector3.up * 2, Quaternion.identity);
          Instantiate(Wallprefab,new Vector3(100f,0,31.85f), Quaternion.identity);
            Debug.Log("wall is there");
        }
        if (Wallprefab == null)
        {
            Debug.Log("NO WALL!!!");
        }
    }

    void LoadAssetsBunndle()
    {
        combinedPath = Path.Combine(Application.streamingAssetsPath, folderPath, fileName);

        if (File.Exists(combinedPath))
        {
            stuff = AssetBundle.LoadFromFile(combinedPath);
            Debug.Log("ahuh stuff is here");
        }
        else
        {
            Debug.Log("i dont see any of those files in the assetsbunndle" + combinedPath);
        }
    }

    void Loadboxes()
    {
        if (box != true)
            return;
        Debug.Log("starting load of box");

        if (stuff == null)
        {
            Debug.Log("assetbundle not found");
            return;
        }

        GameObject BOXprefab = stuff.LoadAsset<GameObject>("Box");

        if (BOXprefab != null)
        {
            Instantiate(BOXprefab, new Vector3(14.95f, 0, 63.37f), Quaternion.identity);
            Instantiate(BOXprefab, new Vector3(15, 0, 53.02f), Quaternion.identity);
            Instantiate(BOXprefab, new Vector3(22.22f, 0, 63.72f), Quaternion.identity);
            Instantiate(BOXprefab, new Vector3(22.53f, 0, 53.38f), Quaternion.identity);
            Debug.Log("box there");
        }
        if (BOXprefab == null)
        {
            Debug.Log("no box in bundle!!!");
        }
    }
}

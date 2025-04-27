using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Assetsbundle : MonoBehaviour
{
    string folderPath = "AssetBundles";
    string fileName = "stuff";
    string combinedPath;

    private AssetBundle stuff; 

    // Start is called before the first frame update
    void Awake()
    {
        LoadAssetsBunndle();
        //Loadprefab();
        Loadboxes();
    }

    void Loadprefab()
    {
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

    }
}

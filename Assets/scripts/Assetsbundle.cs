using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Assetsbundle : MonoBehaviour
{
    string folderPath = "AssetBundles";
    string fileName = "stuff";
    string combinedPath;

    public AssetBundle stuf; 

    // Start is called before the first frame update
    void Start()
    {
        LoadAssetsBunndle();
        Loadprefab();
    }

    void Loadprefab()
    {
        if (stuf == null)
        {
            return; 
        }

        GameObject WallJumpprefab = stuf.LoadAsset<GameObject>("WallJump");   
        if (WallJumpprefab != null)
        {
            Instantiate(WallJumpprefab);
        }
        
    }

    void LoadAssetsBunndle()
    {
        combinedPath = Path.Combine(Application.streamingAssetsPath, folderPath, fileName);

        if (File.Exists(combinedPath))
        {
            stuf = AssetBundle.LoadFromFile(combinedPath);
            Debug.Log("ahuh stuff is here");
        }
        else
        {
            Debug.Log("i dont see any of those files in the assetsbunndle" + combinedPath);
        }
    }
}

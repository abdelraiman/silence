using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Assetsbundle : MonoBehaviour
{
    string folderPath = "assetsbundle";
    string fileName = "stuff";
    string combinedPath;

    public AssetBundle stuf; 

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileSystems : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamingAssetsPath))
        {
            Debug.Log("StreamingAssetsPath: " + streamingAssetsPath);
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets"));
            Debug.Log("Added streamingAssetsPath and can be found at" + streamingAssetsPath);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

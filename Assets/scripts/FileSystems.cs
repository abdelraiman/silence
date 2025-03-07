using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSystems : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string assetPath = Application.dataPath;
        Debug.Log("Asset Path: " + assetPath);

        string persistantPath = Application.persistentDataPath;
        Debug.Log("Persistant Data Path: " + persistantPath);

        string streamingAssetsPath = Application.streamingAssetsPath;
        Debug.Log("StreamingAssetsPath: " + streamingAssetsPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

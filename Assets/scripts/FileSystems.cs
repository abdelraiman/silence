using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class FileSystems : MonoBehaviour
{
    public string filename = "sm.mat";  
    
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
        if (File.Exists(Path.Combine(Application.streamingAssetsPath, "body matirial.mat")))
        {
            Debug.Log("he has SKIN!");
        }
        else
        {
            Debug.Log("APLLAYING SKIN!!!");
            Material newMat = new Material(Shader.Find("Standard"));
            newMat.color = Color.blue;

            GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in objects)
            {
                obj.GetComponent<Renderer>().material = newMat;
            }

        }
    }

    
    void Update()
    {
        
    }
}

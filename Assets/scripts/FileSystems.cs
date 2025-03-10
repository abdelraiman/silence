using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class FileSystems : MonoBehaviour
{
    public string filename = "wall.jpeg";
    public string folderpath = Application.streamingAssetsPath;
    private string CombinedFilePathLocation;
   
    void Start()
    {
        CombinedFilePathLocation = Path.Combine(folderpath, filename);

        Laodtextxr();

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
    void Laodtextxr()
    {
        if (File.Exists(Path.Combine(Application.streamingAssetsPath, "wall.jpeg")))
        {
            Debug.Log("wall looks good");
        }

        if (File.Exists(Path.Combine(Application.streamingAssetsPath, "wall.jpeg"))) 
        {
            byte[] imagebytes = File.ReadAllBytes(CombinedFilePathLocation);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imagebytes);
            //GetComponent<Renderer>().material.mainTexture = texture;

          GameObject[] objects = GameObject.FindGameObjectsWithTag("wall");
          foreach (GameObject obj in objects)
          {
             obj.GetComponent<Renderer>().material.mainTexture = texture;
          }
            Debug.Log("WALLED IS WALL");
        }
        else
        {
            Debug.Log("?what texure");
        }
    }
}
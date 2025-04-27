using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;


public class FileSystems : MonoBehaviour
{ 
    public string filename = "wall.jpeg";
    public string materialDataFileName = "bodyMaterialData.json";
    private string materialDataFilePath;
    private string CombinedFilePathLocation;

    void OnEnable()
    {
        Debug.Log("Ive been enabled");

        CombinedFilePathLocation = Path.Combine(Application.streamingAssetsPath, filename);
        materialDataFilePath = Path.Combine(Application.streamingAssetsPath, materialDataFileName);
        LoadPlayer();
        Laodtextxr();
        
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
                Debug.Log(obj.name);
             obj.GetComponent<Renderer>().material.mainTexture = texture;
          }
            Debug.Log("WALLED IS WALL");
        }
        else
        {
            Debug.Log("?what texure");
        }
    }
    void LoadPlayer()
    {
        Debug.Log("loading player stuff");
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamingAssetsPath))
        {
            Debug.Log("streaming aassets looks good");
            Debug.Log("StreamingAssetsPath: " + streamingAssetsPath);
        }
        else
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets"));
            Debug.Log("Added streamingAssetsPath and can be found at" + streamingAssetsPath);
        }

        if (File.Exists(materialDataFilePath))
        {

            Debug.Log("he has SKIN!");

            string json = File.ReadAllText(materialDataFilePath);
            MaterialData materialData = JsonUtility.FromJson<MaterialData>(json);

            Material material = new Material(Shader.Find(materialData.shader));

            material.color = new Color(materialData.color.r, materialData.color.g, materialData.color.b, materialData.color.a);

            material.SetFloat("_Smoothness", materialData.smoothness);


            GameObject[] objects = GameObject.FindGameObjectsWithTag("PlayerBody");

            foreach (GameObject obj in objects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();

                if (renderer != null)
                {
                    renderer.material = material;
                    Debug.Log("Material applied to: " + obj.name);
                }                 
            }
        }
        else
        {
            Debug.Log("APLLAYING SKIN!!!");
            Material newMat = new Material(Shader.Find("Standard"));
            newMat.color = Color.blue;

            GameObject[] objects = GameObject.FindGameObjectsWithTag("PlayerBody");
            foreach (GameObject obj in objects)
            {
                obj.GetComponent<Renderer>().material = newMat;
            }
        }
    }
    [System.Serializable]
    public class MaterialData
    {
        public string shader;
        public ColorData color;
        public float smoothness;
    }

    [System.Serializable]
    public class ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }
}

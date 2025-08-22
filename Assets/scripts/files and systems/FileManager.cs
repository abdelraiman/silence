using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float playerX;
    public float playerY;
    public float playerZ;
    public float level;
}

public class FileManager : MonoBehaviour
{
    private string savePath;

    void Awake() // Ensure path is set before anything else
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

    }

    public void SaveGame()
    {
        GameObject objects = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = objects.transform.position; 
        float level = 1;
        GameData data = new GameData { playerX = playerPos.x, playerY = playerPos.y,playerZ = playerPos.z, level = level };
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved: " + savePath);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            
            GameData data = JsonUtility.FromJson<GameData>(json);

            GameObject objects = GameObject.FindGameObjectWithTag("Player");
            objects.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        }
        else
        {
            Debug.Log("No Save File Found.");
        }
    }
}

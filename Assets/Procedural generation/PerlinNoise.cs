using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PerlinNoise : MonoBehaviour
{
    public int width;
    public int height;
    public float scale;

    public Terrain terrain;
    public float heightMultiplier = 10;

  
    public void ApplyPerlinNoiseToTerrain()
    {
        width = Random.Range(200, 256);
        height = Random.Range(200, 256);
        scale = Random.Range(20, 50);
        if (terrain == null)
        {
            Debug.Log("no terrain");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;
        float[,] heights = new float[terrainWidth, terrainHeight];

        for (int y = 0; y < terrainHeight; y++)
        {
            for (int x = 0; x < terrainWidth; x++)
            {
                float xCoordinate = (float)x / terrainWidth * scale;
                float yCoordinate = (float)y / terrainHeight * scale;
                float sample = Mathf.PerlinNoise(xCoordinate, yCoordinate);

                heights[x, y] = sample * heightMultiplier / terrainData.size.y;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}

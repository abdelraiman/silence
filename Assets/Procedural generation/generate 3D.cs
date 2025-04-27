using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generate3D : MonoBehaviour
{
    public Texture2D smiletexure;

    public GameObject cube;

    [Header("Spawn veriables")]
    public float RaycastHight;
    public float spacing;
    public float boxpixlesize;
    void Start()
    {
        Spawncoin();
    }

    void Spawncoin()
    {
        Debug.Log("spawncoin");
        if (smiletexure == null)
        {
            Debug.Log(":(");
            return;
        }
        Debug.Log(":)");

        bool[,] OcupiedPixles = new bool[smiletexure.width, smiletexure.height];
        Debug.Log("im here");
        int countCubes = 0;

        for (float y = 0; y < smiletexure.height; y += boxpixlesize)
        {
            for (float x = 0; x < smiletexure.height; x += boxpixlesize)
            {
                if (CanSpawnbox(smiletexure, Mathf.FloorToInt(x), Mathf.FloorToInt(y), OcupiedPixles))
                {
                    MarkOcupied(Mathf.FloorToInt(x), Mathf.FloorToInt(y), OcupiedPixles);

                    Vector3 spawnpos = new Vector3(x * boxpixlesize, RaycastHight, y * boxpixlesize) + transform.position;

                    if (Physics.Raycast(spawnpos, Vector3.down, out RaycastHit hit, RaycastHight * 2))
                    {
                        spawnpos.y = hit.point.y + (cube.transform.localScale.y/ 2); ;
                    }

                    Instantiate(cube, spawnpos, Quaternion.identity);

                    countCubes++;
                    Debug.Log("objects =" + countCubes);
                }
            }
        }
    }
    bool CanSpawnbox(Texture2D image, int startX, int startY, bool[,] occupied)
    {
        int redpixlecounter = 0;

        for (int y = 0; y < Mathf.CeilToInt(boxpixlesize); y++)
        {
            for (int x = 0; x < Mathf.CeilToInt(boxpixlesize); x++)
            {
                int pixleX = startX + x;
                int pixleY = startY + y;

                if (pixleX >= image.width || pixleY >= image.height)
                {
                    continue;
                }

                if (occupied[pixleX, pixleY])
                {
                    return false;
                }

                Color pixelColour = image.GetPixel(pixleX, pixleY);
                if (IsRed(pixelColour))
                {
                    redpixlecounter++;
                }
            }
        }
        return redpixlecounter >= boxpixlesize;
    }
    void MarkOcupied(int startX, int startY, bool[,] occupied)
    {
        for (int y = 0; y < Mathf.CeilToInt(boxpixlesize); y++)
        {
            for (int x = 0; x < Mathf.CeilToInt(boxpixlesize); x++)
            {
                int pixleX = startX + x;
                int pixleY = startY + y;

                if (pixleX >= occupied.GetLength(0) || pixleY >= occupied.GetLength(1))
                {
                    continue;
                }

                occupied[pixleX, pixleY] = true;
            }
        }
    }

    private bool IsRed(Color color)
    {
        return color.r > 0 && color.g < 1 && color.b < 1;
    }
}

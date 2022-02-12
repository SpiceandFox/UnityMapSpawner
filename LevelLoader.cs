using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Texture2D[] Maps;
    private Texture2D Map;
    public ColorToPrefab[] colorToPrefabs;
    private static LevelLoader instance;
    private int activeLevelNumber;
    public GameObject cam;
    private GameObject activeCam;

    private Dictionary<Color32, GameObject> dictionary = new Dictionary<Color32, GameObject>();

    void Start()
    {
        instance = this;
        activeLevelNumber = 0;
        CreateDictionary();
        LoadNextLevel();
        
    }

    private void CreateCamera()
    {
        activeCam = (GameObject)Instantiate(cam);
    }

    public static LevelLoader GetInstance()
    {
        return instance;
    }

    public void LoadNextLevel()
    {
        DeleteExistingLevel();
        //DeleteExistingPlayer();
        DeleteCamera();
        if (activeLevelNumber >= Maps.Length)
        {

            SceneManager.LoadScene("Endscene");
            return;
        }
        Map = Maps[activeLevelNumber];
        CreateNewLevel();
        activeLevelNumber++;
        CreateCamera();
    }

    private void DeleteCamera()
    {
        Destroy(activeCam);
    }

    private void DeleteExistingLevel()
    {
        //Map leeren
        while (this.transform.childCount > 0)
        {
            Transform c = this.transform.GetChild(0);
            c.SetParent(null); //Batman
            Destroy(c.gameObject); 
        }
    }

    private void CreateDictionary()
    {
        foreach (ColorToPrefab colorToPrefab in colorToPrefabs)
        {
            dictionary.Add(colorToPrefab.color, colorToPrefab.prefab);
        }
    }

    private void CreateNewLevel()
    {
        Color32[] pixels = Map.GetPixels32();
        int width = Map.width;
        int height = Map.height;

        //Über alle Pixel loopen
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnPrefabAt(pixels[ y * width + x], x, y);
            }
        }
    }

    private void SpawnPrefabAt(Color32 color, int x, int y)
    {
        if (color.a < 1)
        {
            return;
        }

        if (dictionary.ContainsKey(color))
        {
            GameObject prefab = dictionary[color];
            GameObject gameObject = (GameObject)Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            gameObject.transform.SetParent(this.transform);
        }
        else
        {
            Debug.LogError("Kein Prefab zur Farbe " + color.ToString() + " gefunden");
        }

    }
}

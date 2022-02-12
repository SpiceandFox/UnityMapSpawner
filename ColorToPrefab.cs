using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorToPrefab
{
    public ColorToPrefab()
    {
        this.color = new Color32();
        this.color.a = 255;
    }
    public ColorToPrefab(Color32 color)
    {
        this.color = color;
    }
    public Color32 color;
    public GameObject prefab;
    public GameObject parent;
}

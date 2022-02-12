using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public struct DictionaryObject
{
    public GameObject prefab;
    public GameObject parent;
}
public class SnF_MapSpawner : EditorWindow
{
    #region Preparing
    public Texture2D map;
    public List<ColorToPrefab> colorToPrefabs = new List<ColorToPrefab>();

    protected Dictionary<Color32, DictionaryObject> dictionary = new Dictionary<Color32, DictionaryObject>();
    protected GameObject SnF_Map;

    //GuiLayoutstuff
    protected string tagForParentMap = "SnF_MapTag";
    protected string nameForParentMap = "SnF_Map";
    protected string layerNameForParentMap = "SnFMap_Layer";
    protected int layerForParentMap = 10;
    protected bool advancedSettings = false;
    protected bool showDictionary = true;
    protected Vector2 scrollPos;

    [MenuItem("Window/SnF Map Spawner")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SnF_MapSpawner));
    }
    private void OnEnable()
    {
        scrollPos = Vector2.zero;
        if (colorToPrefabs.Count < 1)
        {
            colorToPrefabs.Add(new ColorToPrefab());
        }
    }
    #endregion

    #region GUI Stuff

    private void OnGUI()
    {
        
        GUILayout.Label("Spawn Map of Prefabs", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("Map");
        map = EditorGUILayout.ObjectField(map, typeof(Texture2D), false) as Texture2D;
        GUILayout.EndVertical();
        showDictionary = EditorGUILayout.Foldout(showDictionary, "Dictionary");
        if (showDictionary)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);
            EditorGUI.indentLevel++;
            for (int i = 0; i < colorToPrefabs.Count; i++)
            {
                colorToPrefabs[i].color = EditorGUILayout.ColorField("Color " + (i+1), colorToPrefabs[i].color);
                colorToPrefabs[i].prefab = EditorGUILayout.ObjectField("Object " + (i+1), colorToPrefabs[i].prefab, typeof(GameObject), false) as GameObject;
                if (GUILayout.Button("Remove"))
                {
                    colorToPrefabs.RemoveAt(i);
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add new Color"))
            {
                colorToPrefabs.Add(new ColorToPrefab());
            }
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(IncorrectMapAttributes());
            if (GUILayout.Button("Get all colors from Map"))
            {
                GetAllColorsFromMap();
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.Space();
        advancedSettings = EditorGUILayout.Foldout(advancedSettings, "Advanced Settings");
        if (advancedSettings)
        {
            EditorGUI.indentLevel++;
            nameForParentMap = EditorGUILayout.TextField("Name for Parent Object ", nameForParentMap);
            tagForParentMap = EditorGUILayout.TextField("Parent Object Tag ", tagForParentMap);
            layerNameForParentMap = EditorGUILayout.TextField("Name for Parent Object Layer ", layerNameForParentMap);
            layerForParentMap = EditorGUILayout.IntField("Name for Parent Object Layer ", layerForParentMap);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(IncorrectMapAttributes());
        if (GUILayout.Button("Spawn Map"))
        {
            SpawnMapObjects();
        }
        EditorGUI.EndDisabledGroup();
    }

    private bool IncorrectMapAttributes()
    {
        //other filter modes mess with the pixel color  
        if (map == null)
        {
            EditorGUILayout.HelpBox("Please Select a Map", MessageType.Warning);
            return true;
        }
        if (!map.filterMode.Equals(FilterMode.Point))
        {
            EditorGUILayout.HelpBox("Filtermode on map has to be set to Point", MessageType.Warning);
            return true;
        }
        //map has to be readable to be looped over
        if (!map.isReadable)
        {
            EditorGUILayout.HelpBox("Map is not set to readable, please set it to readable", MessageType.Warning);
            return true;
        }
        return false;
    }
    private void CreateTag()
    {
        // Open tag manager
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // For get layer property
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        // Adding a Tag
        // First check if it is not already present
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagForParentMap)) { found = true; break; }
        }

        // if not found, add it
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = tagForParentMap;
        }

        // Setting a Layer
        SerializedProperty sp = layersProp.GetArrayElementAtIndex(layerForParentMap);
        if (sp != null) sp.stringValue = layerNameForParentMap;
        // and to save the changes
        tagManager.ApplyModifiedProperties();
    }
    private void GetAllColorsFromMap()
    {
        //contains all colors used in the map
        HashSet<Color32> colors = new HashSet<Color32>();

        //pixels of map
        Color32[] pixels = map.GetPixels32();

        //get all pixels to hashset
        foreach (Color32 color in pixels)
        {
            //transparent colors get skipped 
            if (color.a > 0)
            {
                colors.Add(color);
            }
        }
        foreach (Color32 color in colors)
        {
            //check if editor List already has said color
            if (!colorToPrefabs.Any(x => x.color.Equals(color)))
            {
                colorToPrefabs.Add(new ColorToPrefab(color));
            }
        }
    }
    #endregion

    #region Main logic
    private void SpawnMapObjects()
    {
        //fassade
        if (IncorrectMapAttributes())
        {
            return;
        }
        if (CreateDictionary())
        {
            return;
        } 
        CreateTag();
        DeleteExistingLevel();
        CreateFolderObjects();
        SpawnParenObject();
        CreateNewLevel();
    }

    private void CreateFolderObjects()
    {
        foreach (var dic in dictionary)
        {
            
        }
    }

    private void CreateNewLevel()
    {
        Color32[] pixels = map.GetPixels32();
        int width = map.width;
        int height = map.height;

        //Loop over pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnPrefabAt(pixels[y * width + x], x, y);
            }
        }
    }

    private void DeleteExistingLevel()
    {
        GameObject existingMap = GameObject.FindWithTag(tagForParentMap);
        if (existingMap == null)
        {
            return;
        }
        //Empty map
        while (existingMap.transform.childCount > 0)
        {
            Transform c = existingMap.transform.GetChild(0);
            c.SetParent(null); //become Batman
            DestroyImmediate(c.gameObject);

        }
        DestroyImmediate(existingMap);
    }

    private void SpawnPrefabAt(Color32 color, int x, int y)
    {
        //Spawns Prefab at coordinates and SnF_Map as their parent
        if (color.a < 1)
        {
            return;
        }

        if (dictionary.ContainsKey(color))
        {
            DictionaryObject dictionaryObject = dictionary[color];
            string name = dictionaryObject.prefab.name + '(' + x + ',' + y + ',' + dictionaryObject.prefab.transform.position.z + ')';
            GameObject gameObject = (GameObject)Instantiate(dictionaryObject.prefab, new Vector3(x, y, dictionaryObject.prefab.transform.position.z), Quaternion.identity, dictionaryObject.parent.transform);
            gameObject.name = name;
            gameObject.transform.parent.SetParent(SnF_Map.transform);
            //GameObject prefab = dictionary[color].prefab;
            //string name = prefab.name + '(' + x + ',' + y + ',' + prefab.transform.position.z + ')';
            //GameObject gameObject = (GameObject)Instantiate(prefab, new Vector3(x, y, prefab.transform.position.z), Quaternion.identity);
            //gameObject.name = name;
            //gameObject.transform.SetParent(SnF_Map.transform);
        }
        else
        {
            Debug.LogError("No Prefab for Color " + color.ToString() + " found");
        }

    }

    private void SpawnParenObject()
    {
        
        //for a cleaner scene and a good way to 
        //prevent multiple uploads of the same object
        SnF_Map = new GameObject();
        SnF_Map.name = nameForParentMap;
        SnF_Map.tag = tagForParentMap;
    }

    private bool CreateDictionary()
    {
        //creates new dictionary each time, otherwise changes won't be saved....
        dictionary = new Dictionary<Color32, DictionaryObject>();
        DictionaryObject dictionaryObject = new DictionaryObject();
        int index = 1;
        foreach (var colorToPrefab in colorToPrefabs)
        {
            if (colorToPrefab.prefab == null)
            {
                Debug.LogError("The prefab \"Object " + index 
                    + "\" is not yet assigned");
                return true;
            }
            if (!dictionary.ContainsKey(colorToPrefab.color))
            {
                dictionaryObject.prefab = colorToPrefab.prefab;
                dictionaryObject.parent = new GameObject();
                dictionaryObject.parent.name = colorToPrefab.prefab.name + 's';
                dictionary.Add(colorToPrefab.color, dictionaryObject);
            }
            else
            {
                Debug.LogError("Color " + colorToPrefab.color.ToString() + " at index " + index +
                    " is already defined. \nPlease check your dictionary for duplicate colors");
                return true;
            }
            index++;
        }
        return false;
    }

    #endregion
}

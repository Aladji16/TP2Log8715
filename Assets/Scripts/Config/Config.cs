using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "Config", menuName = "General Config", order = 1)]
public class Config : ScriptableObject {
    private static string dataPath;

    private void OnEnable()
    {
        dataPath = Path.Combine(Application.dataPath, "config.json");
        previousTime = Time.time;
    }

    private float previousTime; 
    public void SaveToJson()
    {
        string jsonString = JsonUtility.ToJson(this);
        Debug.Log("saving to " + dataPath);
        using (StreamWriter streamWriter = File.CreateText(dataPath))
        {
            streamWriter.Write(jsonString);
        }
    }

    public void LoadSave()
    {
#if UNITY_EDITOR
        return;
#pragma warning disable 0162
#endif
        Debug.Log($"Loading config from {dataPath}");
        if (File.Exists(dataPath))
        {
            using (StreamReader streamReader = File.OpenText(dataPath))
            {
                string jsonString = streamReader.ReadToEnd();
                Debug.Log(jsonString);
                JsonUtility.FromJsonOverwrite(jsonString, this);
            }
        }
#if UNITY_EDITOR
#pragma warning restore 0162
#endif
    }

    public enum Shape
    {
        Circle,
    }

    [Serializable]
    public struct ShapeConfig
    {
        public Vector2 initialPos;
        public float size;
        public Shape shape;
        public Color color;
    }

    [SerializeField]
    public int numberOfShapesToSpawn;

    /*
     1 e - 1/2
     4 e - 1/4
     16 e - 1/8
     256 e - 1/16
         
         */

    [SerializeField]
    public float MinSize
    {
        get
        {
            return 1f / (Mathf.Sqrt(numberOfShapesToSpawn)) * 5f;
        }
    }//= 0.1f;
    [SerializeField]
    public float MaxSize
    {
        get
        {
            return MinSize * 1.5f;
        }
    }//= 0.5f;
    [SerializeField]
    public bool headless = false;


    [SerializeField]
    [Tooltip("Key is the name of the system to enable")]
    private StringBoolDictionary _systemsEnabled = StringBoolDictionary.New<StringBoolDictionary>();
    public Dictionary<string, bool> SystemsEnabled { get { return _systemsEnabled.dictionary; } }
}


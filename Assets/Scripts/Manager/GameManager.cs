using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";

    private void Awake()
    {
        filePath = string.Concat(Application.persistentDataPath, "/", saveFileName_1);
        saveData = new SaveData();
        Load();
    }

    private void Save()
    {

    }

    private void Load()
    {


        SetData();
    }

    private void SetData()
    {

    }



    private void OnApplicationQuit()
    {
        Save();
    }
    private void OnApplicationFocus(bool focus)
    {
        if(!focus)
        {
            Save();
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            Save();
        }
    }
}

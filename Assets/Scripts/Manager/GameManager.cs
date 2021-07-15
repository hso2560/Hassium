using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";

    public delegate void LoadingFunc();
    public event LoadingFunc LoadingFuncEvent;

    private PlayerScript player;
    public PlayerScript PlayerSc { get { return player; } }
    public Dictionary<short, PlayerScript> idToMyPlayer;

    private void Awake()
    {
        filePath = string.Concat(Application.persistentDataPath, "/", saveFileName_1);
        saveData = new SaveData();
        Load();
        CreatePool();
        InitData();
    }

    private void InitData()
    {
        idToMyPlayer = new Dictionary<short, PlayerScript>();
        
        for(int i=0; i<saveData.userInfo.characters.Count; i++)
        {
            PlayerScript ps= Resources.Load<GameObject>("Player/" + saveData.userInfo.characters[i].charResoName).transform.GetChild(1).GetComponent<PlayerScript>();
            idToMyPlayer.Add(saveData.userInfo.characters[i].id, ps);
        }
    }

    public void SaveData()
    {
        player.Save();
        saveData.userInfo.curCharResoName = saveData.userInfo.currentChar.charResoName;
        saveData.userInfo.currentPos = player.transform.position;
        saveData.userInfo.currentRot = player.transform.rotation;
        for(int i=0; i< saveData.userInfo.characters.Count; i++)
        {
            if(player.Id == saveData.userInfo.characters[i].id)
            {
                saveData.userInfo.characters[i] = saveData.userInfo.currentChar;
            }
        }
    }
    public void Save()
    {
        SaveData();

        savedJson = JsonUtility.ToJson(saveData);
        byte[] bytes = Encoding.UTF8.GetBytes(savedJson);
        string code = Convert.ToBase64String(bytes);
        //File.WriteAllText(filePath, code);
    }

    private void Load()
    {
        if(File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = Convert.FromBase64String(code);
            savedJson = Encoding.UTF8.GetString(bytes);
            saveData = JsonUtility.FromJson<SaveData>(savedJson);
        }

        SetData();
    }

    private void SetData()
    {
        if(saveData.userInfo.isFirstStart)
        {
            PlayerScript ps = Resources.Load<GameObject>("Player/" + saveData.userInfo.curCharResoName).transform.GetChild(1).GetComponent<PlayerScript>();
            ps.AddInfo();

            saveData.userInfo.isFirstStart = false;
        }
    }

    public void AddCharacter(string path)
    {
        PlayerScript ps = Resources.Load<GameObject>("Player/" + path).transform.GetChild(1).GetComponent<PlayerScript>();
        ps.AddInfo();
        idToMyPlayer.Add(ps.Id, ps);
    }

    public void ChangeCharacter(short id)
    {
        if (id == player.Id || !idToMyPlayer.ContainsKey(id)) return;

        Save();


    }
    
    private void CreatePool()
    {
        
    }   
    
    public void Loading(int index=0)
    {

        if (LoadingFuncEvent != null)
        {
            LoadingFuncEvent.Invoke();

            foreach (LoadingFunc lf in LoadingFuncEvent.GetInvocationList())
            {
                LoadingFuncEvent -= lf;
            }
        }

        UIManager.Instance.LoadingFade(true, index);
    }

    private void Update()
    {
        
    }

    public void SpawnPlayer()
    {
        UserInfo info = saveData.userInfo;

        player = Instantiate(Resources.Load<GameObject>("Player/" + info.curCharResoName),
                             Vector3.zero, Quaternion.identity).transform.GetChild(1).GetComponent<PlayerScript>();

        player.SetData(sceneObjs.joystickCtrl, info.currentPos, info.currentRot);
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

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            SpawnPlayer();
        }
    }
}

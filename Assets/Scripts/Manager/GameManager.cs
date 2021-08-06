using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

//generate lighting �� ���� Sky�� Default Sky�� �ٲٰ� ����. (�� �Ŀ� �ٽ� ���� ������ Sky�� ��ü ��)
public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";

    public delegate void LoadingFunc();
    public event LoadingFunc LoadingFuncEvent;  //�ε�, Ȯ�ι�ư ������ � �Լ��� ó���� �� ���⿡ �־ ��

    private PlayerScript player;
    public PlayerScript PlayerSc { get { return player; } }

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Dictionary<short, PlayerScript> idToMyPlayer;
    public List<PlayerScript> playerList;

    [HideInInspector] public CameraMove camMove;

    public string GetFilePath(string fileName) => string.Concat(Application.persistentDataPath, "/", fileName);

    private void Awake()  //������(1): ī�޶� �ݶ��̴� ��� ���� ����.
    {
        filePath = GetFilePath(saveFileName_1);
        saveData = new SaveData();
        Load();
        CreatePool();  //�̰� ����ٰ� ���� �ؿ� �� ���� ������ ������ ���߿� �����غ��� ������
    }

    private void InitData()
    {
        idToMyPlayer = new Dictionary<short, PlayerScript>();
        playerList = new List<PlayerScript>();

        camMove = sceneObjs.camMove;
    }

    #region ����/�ε�
    public void SaveData()  //�����͸� ����
    {
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            player.Save();
            saveData.userInfo.curCharResoName = saveData.userInfo.currentChar.charResoName;

            saveData.userInfo.currentPos = player.transform.position;
            saveData.userInfo.currentRot = player.transform.rotation;
            saveData.userInfo.curModelRot = player.playerModel.rotation;

            for (int i = 0; i < saveData.userInfo.characters.Count; i++)
            {
                if (player.Id == saveData.userInfo.characters[i].id)
                {
                    saveData.userInfo.characters[i] = saveData.userInfo.currentChar;
                }
            }

            UIManager.Instance.SaveData();
        }
    }
    public void Save()  //������ ���Ͽ� �����͸� �����Ѵ�
    {
        SaveData();

        savedJson = JsonUtility.ToJson(saveData);
        byte[] bytes = Encoding.UTF8.GetBytes(savedJson);
        string code = Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    private void Load()  //�ҷ�����
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
     
    public void SetData()  //�ҷ��� �����ͷ� �����ϱ�
    {
        if(saveData.userInfo.isFirstStart)
        {
            PlayerScript ps = Resources.Load<GameObject>("Player/" + saveData.userInfo.curCharResoName).transform.GetChild(1).GetComponent<PlayerScript>();
            ps.AddInfo();
            saveData.userInfo.currentChar = saveData.userInfo.characters[0];

            saveData.userInfo.isFirstStart = false;
        }
    }

    public void ResetData(short n) // 0: All, 1: UserInfo, 2: Option
    {
        switch(n)
        {
            case 0:
                saveData = new SaveData();
                break;
            case 1:
                saveData.userInfo = new UserInfo();
                break;
            case 2:
                saveData.option = new Option();
                break;


            case 11:
                player.transform.position= new Vector3(-3, -5, 32);  //Test
                break;
        }

        Save();
    }

    #endregion

    #region ĳ����
    public void AddCharacter(string path) //ĳ���� �߰�
    {
        PlayerScript ps;
        try
        {
            ps = Resources.Load<GameObject>("Player/" + path).transform.GetChild(1).GetComponent<PlayerScript>();
        }
        catch
        {
            return;
        }

        if (IsExistCharac(ps.Id))
        {
            return;
        }

        ps.AddInfo();

        PlayerScript _ps = Instantiate(Resources.Load<GameObject>("Player/" + path),
                                         Vector3.zero, Quaternion.identity).transform.GetChild(1).GetComponent<PlayerScript>();
        idToMyPlayer.Add(_ps.Id, _ps);
        playerList.Add(_ps);

        SkillManager.Instance.playerSkills.Add(_ps.skill);
        _ps.parent.SetActive(false);
    }

    public void ChangeCharacter(short id)  //ĳ���� ����
    {
        if (id == player.Id || !IsExistCharac(id)) return;

        GameCharacter gc = GetCharData(id);
        if(gc==null) return;
        
        if (player.skill.isResetIfChangeChar) player.skill.OffSkill();

        Save();

        saveData.userInfo.currentChar = gc;
        saveData.userInfo.curCharResoName = gc.charResoName;

        player.parent.SetActive(false);

        ActiveCharacter(id);
    }

    public GameCharacter GetCharData(short id)  //���̵� ���ؼ� ���̺�� �����Ϳ��� ĳ���� ������ �ҷ�����
    {
        for(int i=0; i<saveData.userInfo.characters.Count; ++i)
        {
            if(saveData.userInfo.characters[i].id==id)
            {
                return saveData.userInfo.characters[i];
            }
        }
        return null;
    }

    public bool IsExistCharac(short id)  
    {
        return idToMyPlayer.ContainsKey(id);
    }
    private void ActiveCharacter(short idx)  //��Ȱ��ȭ�� ĳ���͸� Ȱ��ȭ�Ѵ�.
    {
        player = idToMyPlayer[idx].gameObject.GetComponent<PlayerScript>();
        player.parent.SetActive(true);
        player.SetData(sceneObjs.joystickCtrl, saveData.userInfo.currentPos, saveData.userInfo.currentRot, saveData.userInfo.curModelRot);
        SetPlayer();
    }
    #endregion

    private void CreatePool()  //Ǯ�� �ʿ��� ������Ʈ�� ��ȯ
    {
        
    }   
    
    public void Loading(int index=0)  //�ε��߿� ���� ó����
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

    public void SpawnPlayer()  //ó���� �������� ��� ĳ���� ��ȯ �� ��Ȱ��ȭ
    {
        for(int i=0; i<saveData.userInfo.characters.Count; i++)
        {
            PlayerScript ps= Instantiate(Resources.Load<GameObject>("Player/" + saveData.userInfo.characters[i].charResoName),
                                         Vector3.zero, Quaternion.identity).transform.GetChild(1).GetComponent<PlayerScript>();

            idToMyPlayer.Add(ps.Id, ps);
            playerList.Add(ps);
            ps.parent.SetActive(false);
        }

        ActiveCharacter(saveData.userInfo.currentChar.id);
        
        //sceneObjs.thirdPCam.Follow = player.center;
        //sceneObjs.thirdPCam.LookAt = player.center;
    }

    public void SetPlayer()
    {
        camMove.target = player.center;
        camMove.rotTarget = player.transform;
        camMove.player = player;
    }

    private void Update()  //Resources/Player ���� ���� DefaultPlayer2,3�� �׽�Ʈ �������̹Ƿ� ���߿� �����
    {
        if(Input.GetKeyDown(KeyCode.Alpha2))  //Test Code
        {
            ChangeCharacter(20);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))  //Test Code
        {
            ChangeCharacter(10);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))  //Test Code
        {
            AddCharacter("DefaultPlayer2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))  //Test Code
        {
            AddCharacter("DefaultPlayer3");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))  //Test Code
        {
            ChangeCharacter(30);
        }
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

    public void SceneChange(string name)
    {
        Save();
        sceneObjs.AllReadyFalse();
        //Ǯ ����(���� ��)

        SceneManager.LoadScene(name);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            InitData();
            SpawnPlayer();
        }

        isReady = true;
    }
}

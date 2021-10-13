using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum LoadingType
{
    RESPAWN,
    PLAYERDEATH
}

//generate lighting �� ���� Sky�� Default Sky�� �ٲٰ� ����. (�� �Ŀ� �ٽ� ���� ������ Sky�� ��ü ��)
public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad  //�� ������ ĳ���� ���� ����� ī�޶� ��õ��� �̻��� �� �ٶ󺸴� ������ ����.
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";
    [SerializeField] private float playTime;

    [HideInInspector] public SceneSaveObjects infoSaveObjs;  
    public delegate void LoadingFunc();  //�� �κ� �ּ�ġ�� ���� LoadingFunc�� Action���� �ٲ㼭 �� �� �ִ�.  //Action<�Ű�����,�Ű�����,�Ű�����...> Func<�Ű�����..., ��ȯ��>  �Ű����� ���̵� ����
    //public LoadingFunc loadingFunc;
    public event LoadingFunc LoadingFuncEvent;  //�ε�, Ȯ�ι�ư ������ � �Լ��� ó���� �� ���⿡ �־ ��
    public Dictionary<LoadingType, LoadingFunc> keyToVoidFunction;
    //public Dictionary<int, Action> idToAction;
    public event Action objActionHandle;
    public event Action DeathEvent;
    public event Action quitEvent;

    private PlayerScript player;
    public PlayerScript PlayerSc { get { return player; } }

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Dictionary<short, PlayerScript> idToMyPlayer;
    public List<PlayerScript> playerList; 

    [HideInInspector] public CameraMove camMove;

    [SerializeField] private int maxSystemMsgCount = 5;
    private int systemMsgCount = 0;

    public string GetFilePath(string fileName) => string.Concat(Application.persistentDataPath, "/", fileName);

    private void Awake()  //������(1): ī�޶� �ݶ��̴� ��� ���� ����.
    {
        filePath = GetFilePath(saveFileName_1);
        saveData = new SaveData();
        Load();
    }

    private void InitData()
    {
        infoSaveObjs = sceneObjs.infoSaveObjs;
        CreatePool();

        idToMyPlayer = new Dictionary<short, PlayerScript>();
        playerList = new List<PlayerScript>();
        keyToVoidFunction = new Dictionary<LoadingType, LoadingFunc>();

        keyToVoidFunction.Add(LoadingType.RESPAWN, () => {
            if (player.IsDamageableByFall)
            {
                player.IsDamageableByFall = false;
                player.hp -= Mathf.Clamp(player.hp - 1, 0, 100);
                UIManager.Instance.AdjustFillAmound(UIType.HPFILL, player.hp, player.MaxHp);
            }
            player.transform.position = MapManager.Instance.mapCenterDict[saveData.userInfo.mapIndex].position;
        });
        keyToVoidFunction.Add(LoadingType.PLAYERDEATH, () =>
        {
            PlayerScript p = playerList.Find(p => !p.isDie);
            if (p != null)
            {
                ChangeCharacter(p.Id, true);
                Inventory.Instance.ViewCharacterInfo(p.Id);
            }
            else
            {
                LoadingFuncEvent += () => player.RecoveryHp(player.pData.defaultRespawnHp);
                LoadingFuncEvent += () => player.transform.parent.gameObject.SetActive(false);
                LoadingFuncEvent += keyToVoidFunction[LoadingType.RESPAWN];
                LoadingFuncEvent += () => player.transform.parent.gameObject.SetActive(true);
                UIManager.Instance.LoadingFade(false);
            }
            DeathEvent?.Invoke();
        });

        camMove = sceneObjs.camMove;

        for(int i=0; i<saveData.saveObjDatas.Count; i++)
        {
            int idx = saveData.saveObjDatas[i].index;
            saveData.saveObjDatas[i].SetData(infoSaveObjs.objs[idx]);
            //infoSaveObjs.objDatas[idx].active = false;
        }
    }

    private void Start()
    {
        if(saveData.userInfo.camMinRange!=Vector3.zero)
        {
            camMove.camMinPos = saveData.userInfo.camMinRange;
            camMove.camMaxPos = saveData.userInfo.camMaxRange;
        }
    }

    #region ����/�ε�
    public void SaveData()  //�����͸� ����
    {
        saveData.objActiveInfo.SaveDictionary();
        saveData.npcInfo.SaveDictionary();
        saveData.userInfo.playTime += (int)playTime;
        playTime = 0;
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            if (player != null)
            {
                if (player.skill.isResetIfChangeChar) player.skill.OffSkill();  //�̷��� �Ǹ� ����ڰ� ��õ��� ��׶��� ���·� ��ȯ�ص� ��ų ����
                player.Save();

                //saveData.userInfo.curCharResoName = saveData.userInfo.currentChar.charResoName ?? "DefaultPlayer1";

                saveData.userInfo.currentPos = player.transform.position;
                saveData.userInfo.currentRot = player.transform.rotation;
                saveData.userInfo.curModelRot = player.playerModel.rotation;
            }

            for (int i = 0; i < saveData.userInfo.characters.Count; i++)
            {
                if (player.Id == saveData.userInfo.characters[i].id)
                {
                    saveData.userInfo.characters[i] = saveData.userInfo.currentChar;
                }
            }

            saveData.userInfo.camMinRange = camMove.camMinPos;
            saveData.userInfo.camMaxRange = camMove.camMaxPos;

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
        saveData.objActiveInfo.SetDictionary();
        saveData.npcInfo.SetDictionary();

        if(saveData.userInfo.isFirstStart)
        {
            PlayerScript ps = Resources.Load<GameObject>("Player/DefaultPlayer1").transform.GetChild(1).GetComponent<PlayerScript>();
            ps.AddInfo();
            saveData.userInfo.currentChar = saveData.userInfo.characters[0];

            saveData.userInfo.startDate = DateTime.Now.ToString();
            saveData.userInfo.isFirstStart = false;
        }
        else
        {
            if (saveData.userInfo.currentChar.isDie)
            {
                saveData.userInfo.currentChar.isDie = false;
                saveData.userInfo.currentChar.hp = 100;
            }
        }
    }

    public void ResetData(short n) // 0: All, 1: UserInfo, 2: Option
    {
        switch(n)
        {
            case 0:
                saveData = new SaveData();
                //File.Delete(filePath);
                break;
            case 1:
                saveData.userInfo = new UserInfo();
                break;
            case 2:
                LoadingFuncEvent += () =>
                {
                    player.transform.position = MapManager.Instance.mapCenterDict[0].position;
                    saveData = new SaveData();
                    camMove.ResetRange();
                    //saveData.userInfo.isFirstStart = false;
                    player = null;
                    //�� �̵�
                };
                UIManager.Instance.LoadingFade(false);
                break;

            case 11:
                player.transform.position= new Vector3(-1, -6, 37);  //Test
                break;

            case 15:
                AddCharacter("DefaultPlayer3");  //Test
                ChangeCharacter(30);
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

        GameObject btn = Inventory.Instance.charChangeBtns[(int)(_ps.Id * 0.1f - 1)].gameObject;
        btn.SetActive(true);
        btn.transform.GetChild(0).GetComponent<Text>().text = _ps.CharName;
    }

    public void ChangeCharacter(short id, bool respawn=false)  //ĳ���� ����
    {
        if (id == player.Id || !IsExistCharac(id)) return;
        if (!respawn && (idToMyPlayer[id].isDie || !player.isMovable || player.NoControl) ) return;

        GameCharacter gc = GetCharData(id);   //??null ���� ���� ����
        if(gc==null) return;

        Save();

        saveData.userInfo.currentChar = gc;
        //saveData.userInfo.curCharResoName = gc.charResoName;

        GameObject go = player.parent;
        objActionHandle += () => go.SetActive(false);
        //objActionHandle += go =>  go.SetActive(false); 

        //player.parent.SetActive(false);
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

        //saveData.userInfo.characters.Find(x=>x.id==id) �� ���� ����

        return null;
    }

    public bool IsExistCharac(short id) => idToMyPlayer.ContainsKey(id);
   
    private void ActiveCharacter(short idx)  //��Ȱ��ȭ�� ĳ���͸� Ȱ��ȭ�Ѵ�.
    {
        if (player != null)
        {
            player.skill.Change();
        }

        player = idToMyPlayer[idx].gameObject.GetComponent<PlayerScript>();
        player.parent.SetActive(true);
        player.SetData(sceneObjs.joystickCtrl, saveData.userInfo.currentPos, saveData.userInfo.currentRot, saveData.userInfo.curModelRot);
        SetPlayer();
    }
    #endregion

    private void CreatePool()  //Ǯ�� �ʿ��� ������Ʈ�� ��ȯ
    {
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            PoolManager.CreatePool<Meteor>(sceneObjs.prefabs[0], sceneObjs.environMentGroup, 4);
            PoolManager.CreatePool<SystemTxt>(sceneObjs.prefabs[1],sceneObjs.systemMsgParent, 3);
        }
    }   
    
    public void Loading(int index=0)  //�ε��߿� ���� ó����
    {
        if (LoadingFuncEvent != null)
        {
            LoadingFuncEvent();

            foreach (LoadingFunc lf in LoadingFuncEvent.GetInvocationList())  //Action�� �� ��쿡�� �׳� LoadingFunc�� Action���� �ٲٸ� �ȴ�
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
            ps.isDie = saveData.userInfo.characters[i].isDie;
            ps.parent.SetActive(false);
        }

        ActiveCharacter(saveData.userInfo.currentChar.id);
        
        //sceneObjs.thirdPCam.Follow = player.center;
        //sceneObjs.thirdPCam.LookAt = player.center;
    }

    public void SetPlayer()
    {
        camMove.Setting(player.center, player.transform);
        camMove.player = player;

        ActionFuncHandle();
    }

    public void OpenChest(ChestData chestData)
    {
        saveData.userInfo.myChestList.Add(new ChestData(chestData));
    }

    public void OnSystemMsg(string msg,float time=3f ,int size=50)
    {
        if (systemMsgCount == maxSystemMsgCount) return;

        systemMsgCount++;
        PoolManager.GetItem<SystemTxt>().OnText(msg, time, size, () => systemMsgCount--);
    }

    public void ActionFuncHandle()
    {
        if (objActionHandle == null) return;

        objActionHandle();  //  objActionHandle?.Invoke() == if(objActionHandle!=null) { objActionHandle(); }

        foreach (Action a in objActionHandle.GetInvocationList())
        {
            objActionHandle -= a;
        }
    }

    public bool ContainKeyActiveId(int id) => saveData.objActiveInfo.objActiveKeys.Contains(id);

    private void Update()  
    {
        TestInput();
        playTime += Time.deltaTime;
    }

    public void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))  //Test Code
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
        else if (Input.GetKeyDown(KeyCode.H))
        {
            PoolManager.GetItem<Meteor>();
        }
    }

    private void OnApplicationQuit()
    {
        quitEvent?.Invoke();
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

        PoolManager.ClearItem<Meteor>();

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

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

//generate lighting 할 때는 Sky를 Default Sky로 바꾸고 하자. (그 후에 다시 원래 쓰려던 Sky로 교체 ㄱ)
public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad  //겜 시작후 캐릭터 최초 변경시 카메라가 잠시동안 이상한 곳 바라보는 문제점 있음.
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";
    [SerializeField] private float playTime;

    [HideInInspector] public SceneSaveObjects infoSaveObjs;  
    public delegate void LoadingFunc();  //이 부분 주석치고 밑의 LoadingFunc를 Action으로 바꿔서 할 수 있다.  //Action<매개변수,매개변수,매개변수...> Func<매개변수..., 반환값>  매개변수 없이도 가능
    //public LoadingFunc loadingFunc;
    public event LoadingFunc LoadingFuncEvent;  //로딩, 확인버튼 등으로 어떤 함수를 처리할 때 여기에 넣어서 씀
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

    private void Awake()  //문제점(1): 카메라 콜라이더 없어서 벽을 뚫음.
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

    #region 저장/로드
    public void SaveData()  //데이터를 저장
    {
        saveData.objActiveInfo.SaveDictionary();
        saveData.npcInfo.SaveDictionary();
        saveData.userInfo.playTime += (int)playTime;
        playTime = 0;
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            if (player != null)
            {
                if (player.skill.isResetIfChangeChar) player.skill.OffSkill();  //이렇게 되면 사용자가 잠시동안 백그라운드 상태로 전환해도 스킬 꺼짐
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
    public void Save()  //실제로 파일에 데이터를 저장한다
    {
        SaveData();

        savedJson = JsonUtility.ToJson(saveData);
        byte[] bytes = Encoding.UTF8.GetBytes(savedJson);
        string code = Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    private void Load()  //불러오기
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
     
    public void SetData()  //불러온 데이터로 세팅하기
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
                    //씬 이동
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

    public void SaveObjActiveInfo(int index,bool active) //오브젝트 액티브 상태 저장 (만약 이미 저장한 오브젝트면 bool값만 바꾼다)
    {
        SaveObjData sod = saveData.saveObjDatas.Find(x => x.index == index);
        if (sod != null)
        {
            sod.active = active;
        }
        else
        {
            saveData.saveObjDatas.Add(new SaveObjData(index, SaveObjInfoType.ACTIVE, active));
        }
    }

    #endregion

    #region 캐릭터
    public void AddCharacter(string path) //캐릭터 추가
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

    public void ChangeCharacter(short id, bool respawn=false)  //캐릭터 변경
    {
        if (id == player.Id || !IsExistCharac(id)) return;
        if (!respawn && (idToMyPlayer[id].isDie || !player.isMovable || player.NoControl) ) return;

        GameCharacter gc = GetCharData(id);   //??null 붙일 수도 있음
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

    public GameCharacter GetCharData(short id)  //아이디를 통해서 세이브된 데이터에서 캐릭터 데이터 불러오기
    {
        for(int i=0; i<saveData.userInfo.characters.Count; ++i)
        {
            if(saveData.userInfo.characters[i].id==id)
            {
                return saveData.userInfo.characters[i];
            }
        }

        //saveData.userInfo.characters.Find(x=>x.id==id) 쓸 수도 있음

        return null;
    }

    public bool IsExistCharac(short id) => idToMyPlayer.ContainsKey(id);
   
    private void ActiveCharacter(short idx)  //비활성화된 캐릭터를 활성화한다.
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

    private void CreatePool()  //풀링 필요한 오브젝트들 소환
    {
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            PoolManager.CreatePool<Meteor>(sceneObjs.prefabs[0], sceneObjs.environMentGroup, 4);
            PoolManager.CreatePool<SystemTxt>(sceneObjs.prefabs[1],sceneObjs.systemMsgParent, 3);
        }
    }   
    
    public void Loading(int index=0)  //로딩중에 일을 처리함
    {
        if (LoadingFuncEvent != null)
        {
            LoadingFuncEvent();

            foreach (LoadingFunc lf in LoadingFuncEvent.GetInvocationList())  //Action을 쓸 경우에는 그냥 LoadingFunc를 Action으로 바꾸면 된다
            {
                LoadingFuncEvent -= lf;
            }
        }

        UIManager.Instance.LoadingFade(true, index);
    }

    public void SpawnPlayer()  //처음에 보유중인 모든 캐릭들 소환 후 비활성화
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

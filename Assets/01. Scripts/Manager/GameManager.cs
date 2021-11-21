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
public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad  //원래는 퍼즐옵젝들이랑 플레이어&적 스크립트에는 최상위 객체를 만들어서 그걸 상속받는 식으로 했어야 함
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
    public event Action objActionHandle;  //사실 얜 없어도 될듯
    public event Action DeathEvent;
    public event Action quitEvent;
    public Dictionary<int, Action> eventPointAction = new Dictionary<int, Action>();

    private PlayerScript player;
    public PlayerScript PlayerSc { get { return player; } }

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    public Dictionary<short, PlayerScript> idToMyPlayer;
    public List<PlayerScript> playerList; 

    [HideInInspector] public CameraMove camMove;

    [SerializeField] private int maxSystemMsgCount = 5;
    private int systemMsgCount = 0;
    private readonly float autoMoneyDelay = 100f;  //autoMoneyDelay초에 autoMoney골드만큼 자동으로 벌음
    private WaitForSeconds ammWs;
    private readonly long autoMoney = 2;

    private short onTutoPanel;

    [SerializeField] private short testFastMovement;

    public string GetFilePath(string fileName) => string.Concat(Application.persistentDataPath, "/", fileName);

    private void Awake()  
    {
        filePath = GetFilePath(saveFileName_1);
        saveData = new SaveData();
        Load();
    }

    private void InitData()  //초기 세팅
    {
        infoSaveObjs = sceneObjs.infoSaveObjs;
        CreatePool();
        sceneObjs.gameTexts[7].text = $"{autoMoneyDelay}초가 지날 때마다 {autoMoney}골드만큼 추가됩니다.";

        idToMyPlayer = new Dictionary<short, PlayerScript>();
        playerList = new List<PlayerScript>();
        keyToVoidFunction = new Dictionary<LoadingType, LoadingFunc>();
        SetKeyAndFunc();
        SetEvent_Point();

        camMove = sceneObjs.camMove;
        for(int i=0; i<saveData.saveObjDatas.Count; i++)
        {
            int idx = saveData.saveObjDatas[i].index;
            saveData.saveObjDatas[i].SetData(infoSaveObjs.objs[idx]);
            //infoSaveObjs.objDatas[idx].active = false;
        }

        /*float sens = saveData.option.cameraSensitivity;
        camMove.xSpeed += sens;
        camMove.ySpeed += sens;*/

        ammWs = new WaitForSeconds(autoMoneyDelay);

        if(onTutoPanel == 1)
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "캐릭터를 조작할 수 있습니다.",
                "누르고 있으면 달리기 상태가 됩니다.",
                "점프를 합니다. 달리기를 하는 상태에서 점프를 하면 스테미나가 소모됩니다.",
                "공격을 합니다. 멈춘 상태에서 공격할 수 있습니다.",
                "스킬을 사용합니다.",
                "설정에서 인벤토리 확인, 캐릭터 정보 확인, 획득한 보물 정보 확인, 옵션 기능 등을 사용할 수 있습니다."
            },
            new List<TutoArrowUI>(){
                new TutoArrowUI(0,0,new Vector2(-716f,10f)),
                new TutoArrowUI(1,90f,new Vector2(383f,-357f)),
                new TutoArrowUI(2,90f,new Vector2(580.71f,-357f)),
                new TutoArrowUI(3,0f,new Vector2(483.78f,30f)),
                new TutoArrowUI(4,0f,new Vector2(670.95f,30f)),
                new TutoArrowUI(5,-90,new Vector2(-619.29f, 449f))
            },true);
        }
    }

    private void SetKeyAndFunc()  //dictionary 세팅
    {
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
                LoadingFuncEvent += () =>
                {
                    player.RecoveryHp(player.pData.defaultRespawnHp);
                    //player.transform.parent.gameObject.SetActive(false);
                    keyToVoidFunction[LoadingType.RESPAWN]();
                    foreach(GameCharacter gc in saveData.userInfo.characters)
                    {
                        if(gc.id!=player.Id)
                        {
                            gc.isDie = false;
                            gc.hp = player.pData.defaultRespawnHp;
                            idToMyPlayer[gc.id].isDie = false;
                            idToMyPlayer[gc.id].hp = player.pData.defaultRespawnHp;
                        }
                    }
                    //player.transform.parent.gameObject.SetActive(true);
                    //MapManager.Instance.ResetLivingEnemy();
                };
                UIManager.Instance.LoadingFade(false);
            }
            DeathEvent?.Invoke();
        });
    }

    private void SetEvent_Point()  //어떤 포인트에 닿으면 어떤 이벤트를 실행시킬지 정함
    {
        eventPointAction[0] = () => {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "상호작용 가능한 오브젝트 근처에 가면 상호작용 버튼이 생기고 버튼을 누르면 상호작용을 할 수 있습니다.",
            },
            new List<TutoArrowUI>(){
                new TutoArrowUI(0,-90,new Vector2(262.1f,127.9f))
            });
        };

        eventPointAction[5] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "맵을 돌아다니다보면 퍼즐 요소들을 발견할 수 있고 그것들을 클리어하면 보물상자가 나올 수 있습니다.",
                "보물상자를 열면 경험치, 골드, 아이템 등을 획득할 수 있으며 열쇠는 문을 여는데 중요한 아이템입니다."
            },
            new List<TutoArrowUI>());
        };

        eventPointAction[10] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "NPC를 죽이면 아이템이 드랍되지만 일이 더 어려워질 수도 있어서 죽이지 않는 것이 좋습니다.",
                "맵의 중앙에는 탑이 있으며 열쇠를 이용해서 탑을 오를 수 있습니다.",
                "옵션을 통해서 실제시간 적용을 하지않고 플레이할 수도 있습니다."
            },
            new List<TutoArrowUI>());
            player.RecoveryHp(999);
        };

        eventPointAction[15] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "캐릭터가 여러명이면 한 캐릭터가 사망시 사망하지않은 다른 캐릭터로 자동으로 전환되고 모든 캐릭터가 사망시 모든 캐릭터가 부활합니다."
            },
            new List<TutoArrowUI>());
        };
    }

    private void Start()
    {
        if(saveData.userInfo.mapIndex<40) //아직 튜토맵이라면
        {
            MapManager.Instance.ResetWeather();
            sceneObjs.gameBtns[2].image.sprite = sceneObjs.gameSprites[1];
        }
        /*if(saveData.userInfo.camMinRange!=Vector3.zero)
        {
            camMove.camMinPos = saveData.userInfo.camMinRange;
            camMove.camMaxPos = saveData.userInfo.camMaxRange;
        }*/
        //MapManager.Instance.ChangeSky(saveData.userInfo.skyIndex);
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

            //saveData.userInfo.camMinRange = camMove.camMinPos;
            //saveData.userInfo.camMaxRange = camMove.camMaxPos;

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

            onTutoPanel = 1;
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

    public void ResetData(short n) // 0: All, 1: UserInfo, 2: Option  테스트용
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
                    //camMove.ResetRange();
                    //saveData.userInfo.isFirstStart = false;
                    player = null;
                    //씬 이동
                };
                UIManager.Instance.LoadingFade(false);
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

    public bool IsExistCharac(short id) => idToMyPlayer.ContainsKey(id); //해당 id 캐 있는지
   
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
            PoolManager.CreatePool<SoundPrefab>(sceneObjs.prefabs[2], sceneObjs.poolTrm, 10);
            PoolManager.CreatePool<HPBar>(sceneObjs.prefabs[3], sceneObjs.enemyHPParent, 5);
            PoolManager.CreatePool<Item>(sceneObjs.prefabs[5], sceneObjs.poolTrm, 8);
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

    public void SetPlayer()  //다른 플레이어 참조하는 스크립트에다가 할당
    {
        camMove.Setting(player.center, player.transform);
        camMove.player = player;

        ActionFuncHandle();
    }

    public void OpenChest(ChestData chestData, long money, int exp, ItemData[] rewards) //상자를 열고 보상 받는다. 열은 상자 정보도 저장
    {
        ChestData data = new ChestData(chestData);
        saveData.userInfo.myChestList.Add(data);
        Inventory.Instance.AddTreasure(data);

        saveData.userInfo.money += money;
        
        player.GetExp(exp);

        StringBuilder sb = new StringBuilder();
        {
            sb.Append(money.ToString());
            sb.Append("골드, ");
            sb.Append(exp.ToString());
            sb.Append("경험치, ");
        }

        for (int i=0; i<rewards.Length; i++)
        {
            Inventory.Instance.GetItem(rewards[i]);
            sb.Append(rewards[i].name);
            sb.Append("×1");
            if (i != rewards.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(" 획득");

        PoolManager.GetItem<SystemTxt>().OnText(sb.ToString());
    }

    public void KillNPC()  //안씀
    {
        saveData.userInfo.npcKillCount++;
       /* switch (saveData.userInfo.npcKillCount)
        {
            case 3:
                MapManager.Instance.ChangeSky(0);
                break;
            case 6:
                MapManager.Instance.ChangeSky(1);
                break;
            case 9:
                MapManager.Instance.ChangeSky(4);
                break;
        }*/
    }

    public void OnSystemMsg(string msg,float time=3f ,int size=50)  //시스템 메시지를 띄움 (개수 제한까지 해줌)
    {
        if (systemMsgCount == maxSystemMsgCount) return;

        systemMsgCount++;
        PoolManager.GetItem<SystemTxt>().OnText(msg, time, size, () => systemMsgCount--);
    }

    public void ActionFuncHandle()  //objActionHandle호출 (주로 캐릭터 교체시 사용되는 Action)
    {
        if (objActionHandle == null) return;

        objActionHandle();  //  objActionHandle?.Invoke() == if(objActionHandle!=null) { objActionHandle(); }

        foreach (Action a in objActionHandle.GetInvocationList())
        {
            objActionHandle -= a;
        }
    }

    public bool ContainKeyActiveId(int id) => saveData.objActiveInfo.objActiveKeys.Contains(id);  //해당 아이디의 오브젝트의 상호작용 가능 여부를 저장했는지

    public bool IsContainChest(int id)  //해당 ID의 보물상자를 획득했는지
    {
        for(int i=0; i<saveData.userInfo.myChestList.Count; i++)
        {
            if (saveData.userInfo.myChestList[i].id == id) return true;
        }

        return false;
    }

    private void Update()  
    {
        TestInput();
        playTime += Time.deltaTime;
    }

    public void TestInput()  //개발할 때의 테스트
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha2))  //Test Code
        {
            ChangeCharacter(20);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))  //Test Code
        {
            ChangeCharacter(10);
        }*/
        if (Input.GetKeyDown(KeyCode.Alpha4))  //Test Code
        {
            AddCharacter("DefaultPlayer2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))  //Test Code
        {
            AddCharacter("DefaultPlayer3");
        }
        /*else if (Input.GetKeyDown(KeyCode.Alpha3))  //Test Code
        {
            ChangeCharacter(30);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            PoolManager.GetItem<Meteor>();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            saveData.userInfo.mapIndex = 40;
            MapManager.Instance.ActiveMap(MapType.MAINMAP);
            transform.position = MapManager.Instance.mapCenterDict[40].position;
            //camMove.camMinPos = new Vector3(-1607, -300, -332);
            //camMove.camMaxPos = new Vector3(-610, 300, 664);
        }*/
        if (Input.GetKeyDown(KeyCode.M))
        {
            player.transform.position = MapManager.Instance.mapCenterDict[testFastMovement].position;
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

    public void SceneChange(string name) //씬 교체
    {
        Save();
        sceneObjs.AllReadyFalse();

        PoolManager.ClearAllItem();

        SceneManager.LoadScene(name);
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        StopAllCoroutines();
        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            InitData();
            SpawnPlayer();
            StartCoroutine(AutoMakeMoneyCo());
        }
        else
        {
            StopAllCoroutines();
        }

        isReady = true;
    }

    private IEnumerator AutoMakeMoneyCo()  //자동으로 골드 벌음
    {
        while (true)
        {
            yield return ammWs;  
            saveData.userInfo.money += autoMoney;
        }
    }

    public IEnumerator FuncHandlerCo(float time, Action start ,Action handler, Action complete = null)  //뭔가 갑자기 처리해야할 함수 생기면 여기로 해준다
    {
        float elapsed = 0f;
        start?.Invoke();
        while(elapsed<time)
        {
            yield return null;
            elapsed += Time.deltaTime;
            handler?.Invoke();
        }
        complete?.Invoke();
    }
}

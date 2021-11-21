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
public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad  //������ ����������̶� �÷��̾�&�� ��ũ��Ʈ���� �ֻ��� ��ü�� ���� �װ� ��ӹ޴� ������ �߾�� ��
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
    public event Action objActionHandle;  //��� �� ��� �ɵ�
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
    private readonly float autoMoneyDelay = 100f;  //autoMoneyDelay�ʿ� autoMoney��常ŭ �ڵ����� ����
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

    private void InitData()  //�ʱ� ����
    {
        infoSaveObjs = sceneObjs.infoSaveObjs;
        CreatePool();
        sceneObjs.gameTexts[7].text = $"{autoMoneyDelay}�ʰ� ���� ������ {autoMoney}��常ŭ �߰��˴ϴ�.";

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
                "ĳ���͸� ������ �� �ֽ��ϴ�.",
                "������ ������ �޸��� ���°� �˴ϴ�.",
                "������ �մϴ�. �޸��⸦ �ϴ� ���¿��� ������ �ϸ� ���׹̳��� �Ҹ�˴ϴ�.",
                "������ �մϴ�. ���� ���¿��� ������ �� �ֽ��ϴ�.",
                "��ų�� ����մϴ�.",
                "�������� �κ��丮 Ȯ��, ĳ���� ���� Ȯ��, ȹ���� ���� ���� Ȯ��, �ɼ� ��� ���� ����� �� �ֽ��ϴ�."
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

    private void SetKeyAndFunc()  //dictionary ����
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

    private void SetEvent_Point()  //� ����Ʈ�� ������ � �̺�Ʈ�� �����ų�� ����
    {
        eventPointAction[0] = () => {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "��ȣ�ۿ� ������ ������Ʈ ��ó�� ���� ��ȣ�ۿ� ��ư�� ����� ��ư�� ������ ��ȣ�ۿ��� �� �� �ֽ��ϴ�.",
            },
            new List<TutoArrowUI>(){
                new TutoArrowUI(0,-90,new Vector2(262.1f,127.9f))
            });
        };

        eventPointAction[5] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "���� ���ƴٴϴٺ��� ���� ��ҵ��� �߰��� �� �ְ� �װ͵��� Ŭ�����ϸ� �������ڰ� ���� �� �ֽ��ϴ�.",
                "�������ڸ� ���� ����ġ, ���, ������ ���� ȹ���� �� ������ ����� ���� ���µ� �߿��� �������Դϴ�."
            },
            new List<TutoArrowUI>());
        };

        eventPointAction[10] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "NPC�� ���̸� �������� ��������� ���� �� ������� ���� �־ ������ �ʴ� ���� �����ϴ�.",
                "���� �߾ӿ��� ž�� ������ ���踦 �̿��ؼ� ž�� ���� �� �ֽ��ϴ�.",
                "�ɼ��� ���ؼ� �����ð� ������ �����ʰ� �÷����� ���� �ֽ��ϴ�."
            },
            new List<TutoArrowUI>());
            player.RecoveryHp(999);
        };

        eventPointAction[15] = () =>
        {
            sceneObjs.tutoUI.gameObject.SetActive(true);
            sceneObjs.tutoUI.SetData(new List<string>() {
                "ĳ���Ͱ� �������̸� �� ĳ���Ͱ� ����� ����������� �ٸ� ĳ���ͷ� �ڵ����� ��ȯ�ǰ� ��� ĳ���Ͱ� ����� ��� ĳ���Ͱ� ��Ȱ�մϴ�."
            },
            new List<TutoArrowUI>());
        };
    }

    private void Start()
    {
        if(saveData.userInfo.mapIndex<40) //���� Ʃ����̶��
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

            //saveData.userInfo.camMinRange = camMove.camMinPos;
            //saveData.userInfo.camMaxRange = camMove.camMaxPos;

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

    public void ResetData(short n) // 0: All, 1: UserInfo, 2: Option  �׽�Ʈ��
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
                    //�� �̵�
                };
                UIManager.Instance.LoadingFade(false);
                break;
        }

        Save();
    }

    public void SaveObjActiveInfo(int index,bool active) //������Ʈ ��Ƽ�� ���� ���� (���� �̹� ������ ������Ʈ�� bool���� �ٲ۴�)
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

    public bool IsExistCharac(short id) => idToMyPlayer.ContainsKey(id); //�ش� id ĳ �ִ���
   
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
            PoolManager.CreatePool<SoundPrefab>(sceneObjs.prefabs[2], sceneObjs.poolTrm, 10);
            PoolManager.CreatePool<HPBar>(sceneObjs.prefabs[3], sceneObjs.enemyHPParent, 5);
            PoolManager.CreatePool<Item>(sceneObjs.prefabs[5], sceneObjs.poolTrm, 8);
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

    public void SetPlayer()  //�ٸ� �÷��̾� �����ϴ� ��ũ��Ʈ���ٰ� �Ҵ�
    {
        camMove.Setting(player.center, player.transform);
        camMove.player = player;

        ActionFuncHandle();
    }

    public void OpenChest(ChestData chestData, long money, int exp, ItemData[] rewards) //���ڸ� ���� ���� �޴´�. ���� ���� ������ ����
    {
        ChestData data = new ChestData(chestData);
        saveData.userInfo.myChestList.Add(data);
        Inventory.Instance.AddTreasure(data);

        saveData.userInfo.money += money;
        
        player.GetExp(exp);

        StringBuilder sb = new StringBuilder();
        {
            sb.Append(money.ToString());
            sb.Append("���, ");
            sb.Append(exp.ToString());
            sb.Append("����ġ, ");
        }

        for (int i=0; i<rewards.Length; i++)
        {
            Inventory.Instance.GetItem(rewards[i]);
            sb.Append(rewards[i].name);
            sb.Append("��1");
            if (i != rewards.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(" ȹ��");

        PoolManager.GetItem<SystemTxt>().OnText(sb.ToString());
    }

    public void KillNPC()  //�Ⱦ�
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

    public void OnSystemMsg(string msg,float time=3f ,int size=50)  //�ý��� �޽����� ��� (���� ���ѱ��� ����)
    {
        if (systemMsgCount == maxSystemMsgCount) return;

        systemMsgCount++;
        PoolManager.GetItem<SystemTxt>().OnText(msg, time, size, () => systemMsgCount--);
    }

    public void ActionFuncHandle()  //objActionHandleȣ�� (�ַ� ĳ���� ��ü�� ���Ǵ� Action)
    {
        if (objActionHandle == null) return;

        objActionHandle();  //  objActionHandle?.Invoke() == if(objActionHandle!=null) { objActionHandle(); }

        foreach (Action a in objActionHandle.GetInvocationList())
        {
            objActionHandle -= a;
        }
    }

    public bool ContainKeyActiveId(int id) => saveData.objActiveInfo.objActiveKeys.Contains(id);  //�ش� ���̵��� ������Ʈ�� ��ȣ�ۿ� ���� ���θ� �����ߴ���

    public bool IsContainChest(int id)  //�ش� ID�� �������ڸ� ȹ���ߴ���
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

    public void TestInput()  //������ ���� �׽�Ʈ
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

    public void SceneChange(string name) //�� ��ü
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

    private IEnumerator AutoMakeMoneyCo()  //�ڵ����� ��� ����
    {
        while (true)
        {
            yield return ammWs;  
            saveData.userInfo.money += autoMoney;
        }
    }

    public IEnumerator FuncHandlerCo(float time, Action start ,Action handler, Action complete = null)  //���� ���ڱ� ó���ؾ��� �Լ� ����� ����� ���ش�
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

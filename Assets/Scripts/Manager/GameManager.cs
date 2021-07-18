using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>, ISceneDataLoad
{
    [SerializeField] private SaveData saveData;
    public SaveData savedData { get { return saveData; } }
    private string savedJson, filePath;
    private readonly string saveFileName_1 = "SaveFile01";

    public delegate void LoadingFunc();
    public event LoadingFunc LoadingFuncEvent;  //로딩, 확인버튼 등으로 어떤 함수를 처리할 때 여기에 넣어서 씀

    private PlayerScript player;
    public PlayerScript PlayerSc { get { return player; } }
    public Dictionary<short, PlayerScript> idToMyPlayer;
    public List<GameObject> myPlayerList = new List<GameObject>();  //소환이 한 번이라도 된 캐릭터들의 오브젝트 리스트

    public CameraMove camMove;

    private void Awake()
    {
        filePath = string.Concat(Application.persistentDataPath, "/", saveFileName_1);
        saveData = new SaveData();
        Load();
        CreatePool();
    }

    private void InitData()
    {
        idToMyPlayer = new Dictionary<short, PlayerScript>();
        
        for(int i=0; i<saveData.userInfo.characters.Count; i++)
        {
            PlayerScript ps= Resources.Load<GameObject>("Player/" + saveData.userInfo.characters[i].charResoName).transform.GetChild(1).GetComponent<PlayerScript>();
            idToMyPlayer.Add(saveData.userInfo.characters[i].id, ps);
        }

        camMove = sceneObjs.camMove;
    }

    #region 저장/로드
    public void SaveData()  //데이터를 저장
    {
        if (sceneObjs.ScType != SceneType.MAIN) return;

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

    #region 캐릭터
    public void AddCharacter(string path) //캐릭터 추가
    {
        PlayerScript ps = Resources.Load<GameObject>("Player/" + path).transform.GetChild(1).GetComponent<PlayerScript>();
        ps.AddInfo();
        idToMyPlayer.Add(ps.Id, ps);
    }

    public void ChangeCharacter(short id)  //캐릭터 변경
    {
        if (id == player.Id || !idToMyPlayer.ContainsKey(id)) return;
        Save();

        GameCharacter gc = GetCharData(id);
        if(gc==null)
        {
            return;
        }
        else
        {
            saveData.userInfo.currentChar = gc;
        }
        saveData.userInfo.curCharResoName = gc.charResoName;

        player.parent.gameObject.SetActive(false);

        int num = IsExistCharac(id);

        if(num!=-10)
        {
            ActiveCharacter(num);
        }
        else
        {
            SpawnPlayer();
        }
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
        return null;
    }

    public int IsExistCharac(short id)  //해당 아이디의 캐릭터가 이미 한 번 소환된 적이 있는지 (-10이면 없다는 거)
    {
        PlayerScript ps;
        for(int i=0; i<myPlayerList.Count; ++i)
        {
            ps = myPlayerList[i].GetComponent<PlayerScript>();
            if (ps.Id == id)
            {
                return i;
            }
        }
        return -10;
    }
    private void ActiveCharacter(int idx)  //비활성화된 캐릭터를 활성화한다.
    {
        player = myPlayerList[idx].GetComponent<PlayerScript>();
        player.parent.SetActive(true);
        player.SetData(sceneObjs.joystickCtrl, saveData.userInfo.currentPos, saveData.userInfo.currentRot);
        SetPlayer();
    }
    #endregion

    private void CreatePool()  //풀링 필요한 오브젝트들 소환
    {
        
    }   
    
    public void Loading(int index=0)  //로딩중에 일을 처리함
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
        myPlayerList.Add(player.gameObject);

        //sceneObjs.thirdPCam.Follow = player.center;
        //sceneObjs.thirdPCam.LookAt = player.center;
        SetPlayer();
    }

    public void SetPlayer()
    {
        camMove.target = player.center;
        camMove.rotTarget = player.transform;
        camMove.player = player;
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
        //풀 삭제(사운드 등)

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
    }
}

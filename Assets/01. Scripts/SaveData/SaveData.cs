using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public Option option = new Option();
    public UserInfo userInfo = new UserInfo();
    public SaveClass1<int, bool> objActiveInfo = new SaveClass1<int, bool>();
    public List<SaveObjData> saveObjDatas = new List<SaveObjData>();
    public SaveClass1<short, NPCInfo> npcInfo = new SaveClass1<short, NPCInfo>();
    //public SaveClass1<short, EnemyMapData> enemyMapData = new SaveClass1<short, EnemyMapData>();
}

[Serializable]
public class Option
{
    public float masterSoundSize=0.7f;
    public float soundEffectSize=0.7f;
    public float bgmSize=0f;      //mixer 사용,   -40 ~ 10
    public float distFromCam = -4;  //캐릭터와 카메라 사이의 거리
    public int cameraSensitivity = 30; //회전 감도   1~99
}

[Serializable]
public class UserInfo
{
    public bool isFirstStart = true;
    public string startDate;
    //public string curCharResoName="DefaultPlayer1";
    public long money;
    public short mapIndex = 0;
    public int playTime = 0;
    //public int skyIndex = 3;
    public int npcKillCount = 0;
    //public int chestCount = 0;
    //public int maxItemSlotCount = 30;

    public List<ItemData> itemList = new List<ItemData>();
    public List<GameCharacter> characters = new List<GameCharacter>();
    public GameCharacter currentChar = new GameCharacter();

    public List<ChestData> myChestList = new List<ChestData>();

    public Vector3 currentPos=new Vector3(-1,-6,37);  
    public Quaternion currentRot;
    public Quaternion curModelRot;

    //public Vector3 camMinRange;  //카메라 위치의 최소 범위
    //public Vector3 camMaxRange;  //카메라 위치의 최대 범위
}

[Serializable]
public class GameCharacter
{
    public bool isDie;  

    public short id;
    public short level;  

    public int exp;  
    public int currentMaxExp;  
    public int str;  
    public int def;  
    public int hp;  
    public int maxHp;

    public int statPoint;  //statistics

    public float stamina;  
    public float maxStamina;  

    public float runSpeed;  
    public float jumpPower;
    public float staminaRecoverySpeed;

    public string charName;
    public string charResoName;

    public GameCharacter()
    {

    }
    public GameCharacter(short id, int str, int def, int maxHp, float maxStam, float speed, float jump, float stamRecSpeed, string name, string resoName)
    {
        level = 1;
        exp = 0;
        currentMaxExp = 400;
        statPoint = 0;

        this.id = id;
        this.str = str;
        this.def = def;
        this.maxHp = maxHp;
        this.maxStamina = maxStam;
        this.runSpeed = speed;
        this.jumpPower = jump;
        this.staminaRecoverySpeed = stamRecSpeed;
        this.charName = name;
        this.charResoName = resoName;

        hp = maxHp;
        stamina = maxStamina;
        isDie = false;
    }
}

[Serializable]
public class ChestData
{
    public int id;
    public string name;
    public string explain;
    public string date;
    //public Sprite sprite;

    public ChestData() { }
    public ChestData(ChestData chest)
    {
        id = chest.id;
        name = chest.name;
        explain = chest.explain;

        //sprite = chest.sprite;
        date = DateTime.Now.ToString();
    }
}
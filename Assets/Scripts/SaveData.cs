using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public Option option = new Option();
    public UserInfo userInfo = new UserInfo();
}

[Serializable]
public class Option
{
    public float masterSoundSize=0.7f;
    public float soundEffectSize=0.7f;
    public float bgmSize=0.7f;
}

[Serializable]
public class UserInfo
{
    public bool isFirstStart = true;
    public string curCharResoName="DefaultPlayer1";

    public List<GameCharacter> characters = new List<GameCharacter>();
    public GameCharacter currentChar;

    public Vector3 currentPos=new Vector3(-3,-5,32);  //�ӽ÷� �⺻��ġ�� �̷��� ��
    public Quaternion currentRot;
}

[Serializable]
public class GameCharacter
{
    public short id;
    public short level;

    public int exp;
    public int str;
    public int def;
    public int hp;
    public int maxHp;

    public float stamina;
    public float maxStamina;

    public float runSpeed;
    public float jumpPower;
    public float staminaRecoverySpeed;

    public string charName;
    public string charResoName;

    public GameCharacter(short id, int str, int def, int maxHp, float maxStam, float speed, float jump, float stamRecSpeed, string name, string resoName)
    {
        level = 1;
        exp = 0;

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
    }
}
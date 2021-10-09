using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StringListClass 
{
    public List<string> stringList = new List<string>();
}

[Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string explain;

    public int count; //현재 count개 가지고 있음
    public string spritePath; //템 이미지 경로
    public Sprite sprite; //템 이미지

    public ItemType itemType;

    public ItemData() { }
    public ItemData(ItemData data)
    {
        id = data.id;
        name = data.name;
        explain = data.explain;
        count = data.count;
        spritePath = data.spritePath;
        itemType = data.itemType;
        sprite = data.sprite;
    }
}

[Serializable]
public class InfoPanelData
{
    public GameObject infoPanel;
    public Text nameText;
    public Text explain;
    public Image objImage;
    public Text countText;
    public Button mainBtn;
    public InputField mainInput;
}

[Serializable]
public class EnemyMapData
{
    public short id;

    public int maxCount;
    public int currentCount;

    public int delayH; //리스폰 대기 시간 (단위: 시간)

    public string lastDate;  //마지막 접속 종료 날짜
}

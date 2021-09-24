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
    public Sprite sprite; //템 이미지

    public ItemType itemType;

    public ItemData() { }
    public ItemData(ItemData data)
    {
        id = data.id;
        name = data.name;
        explain = data.explain;
        count = data.count;
        sprite = data.sprite;
        itemType = data.itemType;
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
}

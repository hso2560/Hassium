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

    public int count; //���� count�� ������ ����
    public string spritePath; //�� �̹��� ���
    public Sprite sprite; //�� �̹���

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

    public int delayH; //������ ��� �ð� (����: �ð�)

    public string lastDate;  //������ ���� ���� ��¥
}

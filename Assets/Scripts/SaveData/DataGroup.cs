using System;
using System.Collections.Generic;
using UnityEngine;

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
    public Sprite sprite; //�� �̹���

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

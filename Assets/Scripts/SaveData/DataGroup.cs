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
}

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

    public int count; //현재 count개 가지고 있음
    public Sprite sprite; //템 이미지
}

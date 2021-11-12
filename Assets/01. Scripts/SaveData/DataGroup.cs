using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StringListClass 
{
    public List<string> stringList = new List<string>();
    public bool endAction; //stringList�� ���������� ������ ��ȭ�� ����� �� ���� ������ �Լ��� �ִ°�
}

[Serializable]
public class IndexListClass<T>
{
    public int index;
    public int targetIndex;
    public List<T> list = new List<T>();

    public T GetCurrentItem { get { return list[index]; } }

    public void NextIndex() => index = ++index % list.Count;
    

    public T this[int i]
    {
        get
        {
            return list[i];
        }
        set
        {
            list[i] = value;
        }
    }
}

[Serializable]
public class ItemData
{
    public bool cannotDump;

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
        cannotDump = data.cannotDump;
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
public class SpringObject
{
    public SpringJoint spring;
    public LineRenderer line;
    public LensFlare lens;

    public Transform connectedTr;
    public bool IsPressing;
    public bool bComplete;

    public float interval;
    public float min_springMaxDist;
    public float max_springMaxDist;
}

[Serializable]
public class TutoArrowUI
{
    public int index;
    public float rotationZ;
    public Vector3 rectArrowPos;

    public TutoArrowUI(int index, float rotationZ, Vector3 pos)
    {
        this.index = index;
        this.rotationZ = rotationZ;
        this.rectArrowPos = pos;
    }
}

/*[Serializable]
public class EnemyMapData
{
    public short id;

    public int maxCount;
    public int currentCount;

    public int delayH; //������ ��� �ð� (����: �ð�)

    public string lastDate;  //������ ���� ���� ��¥
}*/
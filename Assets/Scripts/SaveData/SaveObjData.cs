using System.Collections.Generic;
using UnityEngine;
using System;

public enum SaveObjInfoType
{
    TRANSFORM,
    MATERIAL,
    ACTIVE
}

public enum NPCType
{
    INFORMATION, //일방적으로 정보만 준다
    ONLYTALK,  //대화만 가능
    CANNOTFIGHT,  //싸울 수 없음(일방적으로 맞을 수는 있음)
    CANFIGHT //싸울 수 있다
}

[Serializable]
public class SaveClass1<K,V>  //만약 딕셔너리 클래스 여러개 저장할 시 인터페이스 활용해서 하는 것도 좋을듯
{
    public V this[K k]
    {
        get
        {
            return objActiveDic[k];
        }
        set
        {
            if (!objActiveDic.ContainsKey(k))
            {
                //objActiveKeys.Add(k);
                //objActiveValues.Add(value);
                objActiveDic.Add(k, value);
            }
            else
            {
                objActiveDic[k] = value;
            }
        }
    }

    public Dictionary<K, V> objActiveDic = new Dictionary<K, V>();

    public List<K> objActiveKeys = new List<K>();
    public List<V> objActiveValues = new List<V>();

    public void SetDictionary()
    {
        objActiveDic.Clear();

        for(int i=0; i<objActiveKeys.Count; ++i)
        {
            objActiveDic.Add(objActiveKeys[i], objActiveValues[i]);
        }
    }

    public void SaveDictionary()
    {
        objActiveKeys.Clear();
        objActiveValues.Clear();

        foreach(K k in objActiveDic.Keys)
        {
            objActiveKeys.Add(k);
            objActiveValues.Add(objActiveDic[k]);
        }
    }
}

[Serializable]
public class SaveObjData
{
    public SaveObjInfoType soit;
    public int index;

    public PRS prs;
    public ResoNameStr resoName;
    public bool active;

    public SaveObjData() { }

    public SaveObjData(int index, SaveObjInfoType soit, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.index = index;
        this.soit = soit;
        prs = new PRS(position, rotation, scale);
    }

    public SaveObjData(int index, SaveObjInfoType soit, string resourceName)
    {
        this.index = index;
        this.soit = soit;
        resoName = new ResoNameStr(resourceName);
    }

    public SaveObjData(int index, SaveObjInfoType soit, bool active)
    {
        this.index = index;
        this.soit = soit;
        this.active = active;
    }

    public void SetData(GameObject go)
    {
        switch (soit)
        {
            case SaveObjInfoType.TRANSFORM:
                go.transform.position = prs.position;
                go.transform.rotation = prs.rotation;
                go.transform.localScale = prs.scale;
                break;
            case SaveObjInfoType.MATERIAL:
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/" + resoName.resoNameList[0]);
                break;
            case SaveObjInfoType.ACTIVE:
                go.SetActive(active);
                break;
        }
    }
}

[Serializable]
public class NPCInfo
{
    public short id;   //키값
    public string name; 

    public short talkId = 0;  //무슨 대화를 할 차례인지 저장한다
    public List<StringListClass> talkList = new List<StringListClass>();

    public NPCType npcType;
    public bool dead = false;
    public bool isFighting = false;
}

[Serializable]
public class PRS
{
    #region Transform
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    #endregion

    public PRS(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public PRS() { }
}

[Serializable]
public class ResoNameStr
{
    public List<string> resoNameList = new List<string>();

    public ResoNameStr() { }

    public ResoNameStr(params string[] names)
    {
        for(int i=0; i<names.Length; i++)
            resoNameList.Add(names[i]);
    }
}



#region 주석
/*[Serializable]
public class SaveObjDataTrm : SaveObjData  
{
    //public Transform _transform;

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public SaveObjDataTrm(int index, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.index = index;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public override void SetData(GameObject go)   //이렇게 하면 부모의 함수만 호출됨...... 그냥 클래스 하나에 다 묶어야겠다
    {
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
    }
}*/

/*[Serializable]
public class SaveClass11<K,V> : Dictionary<K,V>, ISerializationCallbackReceiver  //인스펙터에도 나온다
{
    public List<K> objActiveKeys = new List<K>();
    public List<V> objActiveValues = new List<V>();

    public void OnAfterDeserialize()
    {
        objActiveKeys.Clear();
        objActiveValues.Clear();

        foreach(KeyValuePair<K,V> pair in this)
        {
            objActiveKeys.Add(pair.Key);
            objActiveValues.Add(pair.Value);
        }
    }

    public void OnBeforeSerialize()
    {
        this.Clear();

        for(int i=0; i<objActiveKeys.Count; ++i)
        {
            this.Add(objActiveKeys[i], objActiveValues[i]);
        }
    }
}*/
#endregion
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SaveObjInfoType
{
    TRANSFORM
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

    #region Transform
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    #endregion

    public SaveObjData(int index, SaveObjInfoType soit, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.index = index;
        this.soit = soit;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public void SetData(GameObject go)
    {
        if (soit == SaveObjInfoType.TRANSFORM)
        {
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.localScale = scale;
        }
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
public class SaveClass1<K,V> : Dictionary<K,V>, ISerializationCallbackReceiver  //인스펙터에도 나온다
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
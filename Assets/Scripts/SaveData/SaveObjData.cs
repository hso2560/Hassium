
using UnityEngine;
using System;

public enum SaveObjInfoType
{
    TRANSFORM
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

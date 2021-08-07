
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

    public override void SetData(GameObject go)   //�̷��� �ϸ� �θ��� �Լ��� ȣ���...... �׳� Ŭ���� �ϳ��� �� ����߰ڴ�
    {
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
    }
}*/

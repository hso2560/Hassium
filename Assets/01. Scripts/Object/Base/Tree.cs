using DG.Tweening;
using UnityEngine;

public class Tree : MonoBehaviour, IDamageable
{
    public TreeData treeData;

    public void Death(){}

    public void OnDamaged(int damage, Vector3 hitNormal, float force, bool useDef)
    {
        transform.DOShakePosition(treeData.shakeDuration, treeData.shakeStrength);
        AddWork();
    }

    public virtual void AddWork() //데미지 받고 나서 추가작업 필요하면 이걸로
    {

    }
}

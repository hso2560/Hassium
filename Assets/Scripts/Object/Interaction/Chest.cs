using UnityEngine;
using DG.Tweening;

public class Chest : ObjData
{
    [SerializeField] ChestData chestData;
    [SerializeField] Animator animator;
    [SerializeField] int index;

    //[SerializeField] bool autoIndexFind=false;

    private void Start()
    {
        //if (autoIndexFind) index = GameManager.Instance.infoSaveObjs.objs.IndexOf(gameObject);
        chestData.id = id;
        chestData.name = name;
        chestData.explain = explain;
        GameManager.Instance.SaveObjActiveInfo(index, true);
    }

    public override void Interaction()
    {
        animator.SetTrigger("open");
        transform.GetChild(1).gameObject.SetActive(true);  //effect
        GameManager.Instance.SaveObjActiveInfo(index, false);
        GameManager.Instance.OpenChest(chestData);

        Sequence seq = DOTween.Sequence();
        seq.PrependInterval(5);
        seq.Append(transform.DORotateQuaternion(Quaternion.Euler(0,-179,0), 1));
        seq.Join(transform.DOScale(Vector3.zero, 1));
        seq.AppendCallback(() => gameObject.SetActive(false));
        seq.Play();
    }
}

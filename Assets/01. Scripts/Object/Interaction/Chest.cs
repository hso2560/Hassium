using UnityEngine;
using DG.Tweening;

public class Chest : ObjData
{
    [SerializeField] ChestData chestData;
    [SerializeField] Animator animator;
    [SerializeField] int index;

    public float rotateY = -179f;

    bool open = false;
    //[SerializeField] bool autoIndexFind=false;

    [Header("Reward")]
    [SerializeField] private ItemData[] rewardItems;
    [SerializeField] private long rewardMoney;
    [SerializeField] private int rewardExp;

    private void Start()
    {
        //if (autoIndexFind) index = GameManager.Instance.infoSaveObjs.objs.IndexOf(gameObject);
        chestData.id = id;
        chestData.name = objName;
        //chestData.explain = explain;
        GameManager.Instance.SaveObjActiveInfo(index, true);
    }

    public override void Interaction()
    {
        if (open) return;
        open = true;

        animator.SetTrigger("open");
        transform.GetChild(1).gameObject.SetActive(true);  //effect
        GameManager.Instance.SaveObjActiveInfo(index, false);
        GameManager.Instance.OpenChest(chestData, rewardMoney, rewardExp, rewardItems);

        Sequence seq = DOTween.Sequence();
        seq.PrependInterval(5);
        seq.Append(transform.DORotateQuaternion(Quaternion.Euler(0,rotateY,0), 1));
        seq.Join(transform.DOScale(Vector3.zero, 1));
        seq.AppendCallback(() => gameObject.SetActive(false));
        seq.Play();
    }
}

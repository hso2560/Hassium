using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    public short id;
    public bool timeStop;

    public Image img;
    public CanvasGroup cvsGroup;

    [SerializeField] private Vector3 targetVec = new Vector3(1,1,1);
    [SerializeField] private float time1 = 0.5f, time2 = 0.3f;
    [SerializeField] private Ease ease = Ease.Linear;

    /*private void Awake()
    {
        //ResetData();
    }*/

    private void OnEnable()
    {
        Sequence seq = DOTween.Sequence();

        if(id==0)
        {
            cvsGroup.DOFade(1, time2);
            seq.Append(transform.DOScale(targetVec, time1).SetEase(ease));
            seq.AppendCallback(() =>
            {
                Time.timeScale = 0;
            });
            seq.Play();
        }
    }

    private void OnDisable()
    {
        if (timeStop) Time.timeScale = 1;

        ResetData();
    }

    private void ResetData()
    {
        if (id == 0)
        {
            transform.localScale = Vector3.zero;
            cvsGroup.alpha = 0;
        }
    }
}

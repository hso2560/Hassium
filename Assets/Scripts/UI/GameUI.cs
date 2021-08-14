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

    private void OnEnable()
    {
        if (id == 0)
            UIAnimation(targetVec, () => { Time.timeScale = 0; }, 1, time2, time1);
    }

    private void OnDisable()
    {
        UIManager.Instance.stackUI.Remove(gameObject);
    }

    public void ResetData()
    {
        if (id == 0)
        {
            Time.timeScale = 1;
            UIAnimation(Vector3.zero, () => { gameObject.SetActive(false); }, 0, time2, time1);
        }
    }

    private void UIAnimation(Vector3 vValue, TweenCallback tc ,params float[] fValues)
    {
        Sequence seq = DOTween.Sequence();

        if(id==0)
        {
            cvsGroup.DOFade(fValues[0], fValues[1]);
            seq.Append(transform.DOScale(vValue, fValues[2]).SetEase(ease));
            seq.AppendCallback(tc);
            seq.Play();
        }
    }
}

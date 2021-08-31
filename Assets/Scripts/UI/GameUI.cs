using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    public short id;
    public bool timeStop;
    public bool bSameBtnOff = false;
    public bool noStackUI = false;

    public Image img;
    public CanvasGroup cvsGroup;

    [SerializeField] private Vector3 targetVec = new Vector3(1,1,1);
    [SerializeField] private float time1 = 0.5f, time2 = 0.3f;
    [SerializeField] private Ease ease = Ease.Linear;
    private int idx = -1;

    public PRS prs;

    private void Awake()
    {
        prs = new PRS(transform.position, transform.rotation, transform.localScale);
    }

    private void OnEnable()
    {
        if (idx == -1)
            idx = UIManager.Instance.sceneObjs.ui.IndexOf(gameObject);

        switch (id)
        {
            case 0:
                UIAnimation(targetVec, () => { Time.timeScale = 0; }, 1, time2, time1);
                break;
            case 10:
                if(UIManager.Instance.curMenuPanel!=gameObject)
                {
                    GameObject o = UIManager.Instance.curMenuPanel;
                    o.GetComponent<GameUI>().img.gameObject.SetActive(false);
                    o.SetActive(false);

                    img.gameObject.SetActive(true);
                    UIManager.Instance.curMenuPanel = gameObject;
                }
                break;
            case 20:
                UIAnimation(targetVec, () => { }, time1, time2);
                break;
        }
    }

    private void OnDisable()
    {
        UIManager.Instance.stackUI.Remove(gameObject);
    }

    public void ResetData(int num=-3)
    {
        if(!bSameBtnOff && idx==num)
            return;

        switch (id)
        {
            case 0:
                Time.timeScale = 1;
                UIAnimation(Vector3.zero, () => { gameObject.SetActive(false); }, 0, time2, time1);
                break;
            case 20:
                UIAnimation(prs.rotation.eulerAngles, null, time1, time2);
                break;
        }
    }

    private void UIAnimation(Vector3 vValue, TweenCallback tc ,params float[] fValues)
    {
        Sequence seq = DOTween.Sequence();

        switch (id)
        {
            case 0:
                cvsGroup.DOFade(fValues[0], fValues[1]);
                seq.Append(transform.DOScale(vValue, fValues[2]).SetEase(ease));
                seq.AppendCallback(tc);
                seq.Play();
                break;
            case 20:
                transform.DOKill();
                cvsGroup.DOKill();
                transform.DORotate(targetVec, fValues[0]);
                cvsGroup.DOFade(fValues[1], fValues[0]);
                break;
        }
    }
}

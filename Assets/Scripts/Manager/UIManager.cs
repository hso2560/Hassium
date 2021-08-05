using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoSingleton<UIManager>, ISceneDataLoad
{
    [HideInInspector] public List<Ease> gameEases;
    [HideInInspector] public Image LoadingImg;
    [HideInInspector] public Image crosshairImg;
    [HideInInspector] public List<InteractionBtn> interactionBtns;

    public List<GameObject> beforeItrObjs = new List<GameObject>();

    private Canvas mainCvs, touchCvs;

    private Color noColor;
    private InteractionBtn interBtn = null;

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    private void Awake()
    {
        noColor = new Color(0, 0, 0, 0);
    }

    public void LoadingFade(bool isFadeIn, int index=0)  //페이드인 페이드아웃
    {
        Sequence seq = DOTween.Sequence();
        LoadingImg.gameObject.SetActive(true);

        seq.Append(LoadingImg.DOColor(isFadeIn ? noColor : Color.black, 1).SetEase(gameEases[index]));
        seq.AppendCallback(() =>
        {
            if(isFadeIn)
               LoadingImg.gameObject.SetActive(false);
            else
            {
                GameManager.Instance.Loading();
            }
        });
        seq.Play();
    }

    public void ActiveItrBtn(ObjData od)
    {
        short i;

        if (!od.active)
        {
            for (i = 0; i < interactionBtns.Count; ++i)
            {
                if (interactionBtns[i].data == od)
                {
                    OffInterBtn(interactionBtns[i]);
                    break;
                }
            }
            return;
        }

        for(i=0; i<interactionBtns.Count; ++i)
        {
            if (interactionBtns[i].data == od) return;
        }

        interBtn = null;
        for(i=0; i<interactionBtns.Count; ++i)
        {
            if (!interactionBtns[i].gameObject.activeSelf)
            {
                interBtn = interactionBtns[i];
                break;
            }
        }

        if (interBtn == null) return;

        interBtn.data = od;
        beforeItrObjs.Add(od.gameObject);
        interBtn.transform.GetChild(0).GetComponent<Text>().text = od.objName;
        interBtn.gameObject.SetActive(true);
        interBtn.cvs.DOFade(1, 0.4f);
    }

    public void OffInterBtn(InteractionBtn ib = null)
    {
        if (ib != null)
        {
            beforeItrObjs.Remove(ib.data.gameObject);
            ib.data = null;

            Sequence seq = DOTween.Sequence();

            seq.Append(ib.cvs.DOFade(0, 0.3f));
            seq.AppendCallback(() =>
            {
                ib.gameObject.SetActive(false);
            });
            seq.Play();
        }
        else
        {
            for (short i = 0; i < interactionBtns.Count; i++)
            {
                if (interactionBtns[i].gameObject.activeSelf)
                    OffInterBtn(interactionBtns[i]);
            }
        }
    }

    public void DisableItrBtn(Collider[] cols)
    {
        bool exist;

        for(int i=0; i<beforeItrObjs.Count; i++)
        {
            exist = false;

            for(int j=0; j<cols.Length; j++)
            {
                if(beforeItrObjs[i]==cols[j].gameObject)
                {
                    exist = true;
                    break;
                }
            }

            if(!exist)
            {
                for (int k = 0; k < interactionBtns.Count; k++)
                {
                    if (interactionBtns[k].data.gameObject == beforeItrObjs[i])
                    {
                        OffInterBtn(interactionBtns[k]);
                        break;
                    }
                }
            }
        }
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        UIManager[] managers = FindObjectsOfType<UIManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        LoadingImg = this.sceneObjs.gameImgs[0];
        gameEases = new List<Ease>(this.sceneObjs.gameEases);

        if(this.sceneObjs.ScType==SceneType.MAIN)
        {
            crosshairImg = this.sceneObjs.gameImgs[1];

            mainCvs = this.sceneObjs.cvses[0];
            touchCvs = this.sceneObjs.cvses[1];

            interactionBtns = new List<InteractionBtn>(this.sceneObjs.itrBtns);
        }

        isReady = true;
        //LoadingFade(true, 0);
    }
}

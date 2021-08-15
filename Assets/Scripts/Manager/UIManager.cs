using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum UIType
{
    DIST_FROM_CAM,
    MASTER_SOUND,
    BGM_SIZE,
    SOUND_EFFECT
}

public class UIManager : MonoSingleton<UIManager>, ISceneDataLoad
{
    [HideInInspector] public List<Ease> gameEases;
    [HideInInspector] public Image LoadingImg;
    [HideInInspector] public Image crosshairImg;
    //[HideInInspector] public Image infoPanel;
    [HideInInspector] public List<InteractionBtn> interactionBtns;

    public List<GameObject> beforeItrObjs = new List<GameObject>();
    public List<GameObject> stackUI = new List<GameObject>();
    public GameObject curMenuPanel;

    private Canvas mainCvs, touchCvs, infoCvs;
    private Slider camSlider;
    private Button menuBtn;

    private Color noColor;

    private InteractionBtn interBtn = null;
    private CameraMove camMove;

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

    public void AdjustSlider(UIType t) 
    {
        switch (t)
        {
            case UIType.DIST_FROM_CAM:
                camMove.Offset.z = camSlider.value;
                break;
        }
    }

    public void OnClickUIButton(int num)
    {
        if (num < 0) return;

        GameObject o = sceneObjs.ui[num];
        GameUI gu = o.GetComponent<GameUI>();

        if(!o.activeSelf)
        {
            o.SetActive(true);
            stackUI.Add(sceneObjs.ui[num]);

            if (gu != null && gu.noStackUI)
                stackUI.RemoveAt(stackUI.Count - 1);
        }
        else
        {
            if (gu != null) gu.ResetData(num);
            else
            {
                o.SetActive(false);
                stackUI.Remove(o);
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
            //infoPanel = this.sceneObjs.gameImgs[2];

            mainCvs = this.sceneObjs.cvses[0];
            touchCvs = this.sceneObjs.cvses[1];
            infoCvs = this.sceneObjs.cvses[2];
            camSlider = this.sceneObjs.camSlider;
            curMenuPanel = this.sceneObjs.ui[1];

            interactionBtns = new List<InteractionBtn>(this.sceneObjs.itrBtns);
            menuBtn = this.sceneObjs.gameBtns[0];

            camMove = this.sceneObjs.camMove;

            InitData();
        }

        isReady = true;
        //LoadingFade(true, 0);
    }
    
    private void InitData()
    {
        camSlider.minValue = camMove.zDistMax;
        camSlider.maxValue = camMove.zDistMin;

        camSlider.onValueChanged.AddListener((data) =>
        {
            AdjustSlider(UIType.DIST_FROM_CAM);
        });
      
    }

    private void Start()
    {
        if (sceneObjs.ScType == SceneType.MAIN)
        {
            Option op = GameManager.Instance.savedData.option;

            camSlider.value = op.distFromCam;
        }
    }

    private void Update()
    {
        _Input();
    }

    private void _Input()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(stackUI.Count>0)
               OnClickUIButton(sceneObjs.ui.IndexOf(stackUI[stackUI.Count - 1]));
        }
    }

    public void SaveData()
    {
        Option op = new Option();
        op.distFromCam = camSlider.value;
        GameManager.Instance.savedData.option = op;
    }
}

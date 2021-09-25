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
    SOUND_EFFECT,
    HPFILL
}

public class UIManager : MonoSingleton<UIManager>, ISceneDataLoad
{
    [HideInInspector] public List<Ease> gameEases;
    [HideInInspector] public Image LoadingImg;
    [HideInInspector] public Image crosshairImg;
    [HideInInspector] public Image hpFillImg;
    //[HideInInspector] public Image infoPanel;
    [HideInInspector] public List<InteractionBtn> interactionBtns;

    public List<GameObject> beforeItrObjs = new List<GameObject>();
    public List<GameObject> stackUI = new List<GameObject>();
    public List<Color> uiColors;
    public GameObject curMenuPanel;

    private Canvas mainCvs, touchCvs, infoCvs;
    private CanvasGroup hpFillCvsg;
    private Slider camSlider;
    private Button menuBtn;

    private Color noColor;

    private InteractionBtn interBtn = null;
    private CameraMove camMove;

    public Queue<bool> uiChangeQueue = new Queue<bool>();  //화면의 UI 변화, 위치, 애니메이션 등이 실행/변화중인지 체크하기 위함. 비어있어야 변화 가능
    
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

    #region IteractionBtn
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
            /*interactionBtns.ForEach(x => {
                if(x.data==od) OffInterBtn(x);
            });*/

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

        beforeItrObjs.ForEach(x =>
        {
            exist = false;

            for (int j = 0; j < cols.Length; j++)
            {
                if (x == cols[j].gameObject)
                {
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                OffInterBtn(interactionBtns.Find(k => k.data.gameObject == x));
            }
        });

        #region 주석
        /*for(int i=0; i<beforeItrObjs.Count; i++)
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
        }*/
        #endregion
    }
    #endregion

    #region UI Fill
    public void AdjustFillAmound(UIType t, float current=0, float max=0)
    {
        switch(t)
        {
            case UIType.HPFILL:
                hpFillImg.DOFillAmount(current / max, 0.3f);

                if (current < max / 6) hpFillImg.color = uiColors[2];
                else if (current < max / 2) hpFillImg.color = uiColors[1];
                else hpFillImg.color = uiColors[0];

                break;
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
    #endregion

    public void OnClickUIButton(int num)
    {
        if (num < 0 || (uiChangeQueue.Count!=0 && num!=6)) return;

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
            UIQueue(true); 
            if (gu != null) gu.ResetData(num);
            else
            {
                o.SetActive(false);
                stackUI.Remove(o);
                UIQueue();
            }
        }
    }

    public void UIQueue(bool enqueue=false)
    {
        if(enqueue)
        {
            uiChangeQueue.Enqueue(true);
            return;
        }

        if (uiChangeQueue.Count > 0) uiChangeQueue.Dequeue();
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
            hpFillImg = this.sceneObjs.gameImgs[3];
            hpFillCvsg = hpFillImg.GetComponent<CanvasGroup>();
            //infoPanel = this.sceneObjs.gameImgs[2];

            mainCvs = this.sceneObjs.cvses[0];
            touchCvs = this.sceneObjs.cvses[1];
            infoCvs = this.sceneObjs.cvses[2];
            camSlider = this.sceneObjs.camSlider;
            curMenuPanel = this.sceneObjs.ui[1];

            interactionBtns = new List<InteractionBtn>(this.sceneObjs.itrBtns);
            uiColors = new List<Color>(this.sceneObjs.gameColors);
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

            SetData();
        }
    }

    public void SetData()
    {
        sceneObjs.gameTexts[0].text = GameManager.Instance.savedData.userInfo.money.ToString();  //돈 텍스트 업데이트
    }

    private void Update()
    {
        _Input();
        //deleteValue = uiChangeQueue.Count;
    }

    private void _Input()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && uiChangeQueue.Count==0)
        {
            if (stackUI.Count > 0)
            {
                OnClickUIButton(sceneObjs.ui.IndexOf(stackUI[stackUI.Count - 1]));
            }
            else
            {
                OnClickUIButton(8);
            }
        }
    }

    public void OnSystemPanel(string msg, int fontSize=69)
    {
        OnClickUIButton(6);
        Text t = sceneObjs.ui[6].transform.GetChild(0).GetComponent<Text>();
        t.text = msg;
        t.fontSize = fontSize;
    }

    public void SaveData()
    {
        Option op = new Option();
        op.distFromCam = camSlider.value;
        GameManager.Instance.savedData.option = op;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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

    public event Action timeOverEvent;
    public event Action clearEvent;
    private bool isTimer;
    private float remainingTime;
    [HideInInspector] public bool runningMission = false;
    [HideInInspector] public GameObject missionObj;

    [HideInInspector] public Text objExplainText;
    private Text hpText, timerText, missionCntText;

    public List<GameObject> beforeItrObjs = new List<GameObject>();
    public List<GameObject> stackUI = new List<GameObject>();
    public List<Color> uiColors;
    public GameObject curMenuPanel, timerParent;

    private Canvas mainCvs, touchCvs, infoCvs;
    private CanvasGroup hpFillCvsg;
    private Slider camSlider;
    private Button menuBtn;

    private Color noColor;

    private InteractionBtn interBtn = null;
    private CameraMove camMove;
    [HideInInspector] public Camera mainCam;

    public Queue<bool> uiChangeQueue = new Queue<bool>();  //화면의 UI 변화, 위치, 애니메이션 등이 실행/변화중인지 체크하기 위함. 비어있어야 변화 가능

    private List<GameObject> npcTalkImgs;

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
        seq.InsertCallback(0.2f, () =>
        {
            if (!isFadeIn)
            {
                SoundManager.Instance.PlaySoundEffect(SoundEffectType.FADEOUT);
            }
        });
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
    public void ActiveItrBtn(ObjData od)  //어떤 오브젝트에 대한 상호작용 버튼 띄움
    {
        short i;

        if (!od.active)
        {
            for (i = 0; i < interactionBtns.Count; ++i)  
            {
                if (interactionBtns[i].data == od)
                {
                    OffInterBtn(interactionBtns[i]);   //갑자기 상호작용 불가능 상태 되면 버튼 꺼줌
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
            if (interactionBtns[i].data == od) return;  //이미 해당 옵젝이 상호작용 버튼들 중 하나에 등록되어있으면 리턴
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

        if (interBtn == null) return;  //버튼 자리 없으면 리턴

        interBtn.data = od;
        beforeItrObjs.Add(od.gameObject);
        interBtn.transform.GetChild(0).GetComponent<Text>().text = od.objName;

        if (od.transform.CompareTag("Object"))  //옵젝 설명 띄워준다
        {
            objExplainText.gameObject.SetActive(true);
            objExplainText.text = od.explain;
            objExplainText.transform.DOScaleX(1, 0.3f);
        }

        interBtn.gameObject.SetActive(true);
        interBtn.cvs.DOFade(1, 0.4f);
    }

    public void OffInterBtn(InteractionBtn ib = null)  //상호작용 버튼 꺼줌
    {
        if (ib != null)  //특정 버튼만 꺼줌
        {
            beforeItrObjs.Remove(ib.data.gameObject);
            ib.data = null;
            ib.cvs.DOFade(0, 0.3f).OnComplete(() => ib.gameObject.SetActive(false));
        }
        else  //모든 버튼 꺼줌
        {
            for (short i = 0; i < interactionBtns.Count; i++)
            {
                if (interactionBtns[i].gameObject.activeSelf)
                    OffInterBtn(interactionBtns[i]);
            }
        }

        if (objExplainText.gameObject.activeSelf)
        {
            objExplainText.transform.DOScaleX(0, 0.3f).OnComplete(()=> objExplainText.gameObject.SetActive(false));
        }
    }

    public void DisableItrBtn(Collider[] cols)  //현재 플레이어 주변의 옵젝들과 상호작용 버튼으로 나온 옵젝들을 비교해서 플레이어 주변에 없지만 버튼은 남아있는 것들을 조사해서 꺼준다
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
                OffInterBtn(interactionBtns.Find(k => k.gameObject.activeSelf && k.data.gameObject == x));
                //반드시 k.gameObject.activeSelf값을 먼저 검사해야함(&&의 왼쪽에 배치)
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

                hpText.text = string.Format("HP: {0}/{1}", current, max);

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

    public void OnClickUIButton(int num)  //어떤 버튼을 클릭하면 어떤 UI가 열리나
    {
        if (num < 0 || (uiChangeQueue.Count!=0 && num!=6)) return;

        GameObject o = sceneObjs.ui[num];
        GameUI gu = o.GetComponent<GameUI>();
        SpecificProcessing(num);

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

        SoundManager.Instance.PlaySoundEffect(SoundEffectType.MENUCLICK);
    }

    public void UIQueue(bool enqueue=false)  //UI애니메이션 적용 중에 다른 UI처리가 가능한 것을 방지하기 위해서 uiChangeQueue가 비어있어야 처리 가능.
    {
        if(enqueue)
        {
            uiChangeQueue.Enqueue(true);
            return;
        }

        if (uiChangeQueue.Count > 0) uiChangeQueue.Dequeue();
    }

    private void SpecificProcessing(int num)  //UI끄고 킬 때 특정 UI를 끄고 킬 때의 처리
    {
        switch(num)
        {
            case 0:
                Inventory.Instance.UpdateCharInfoUI();
                break;
            case 4:
                sceneObjs.gameTexts[0].text = GameManager.Instance.savedData.userInfo.money.ToString() + " 골드";
                break;
            case 10:
                Inventory.Instance.OnClickReinforceBtn();
                sceneObjs.ui[3].SetActive(sceneObjs.ui[10].activeSelf);
                break;
            case 12:
                Inventory.Instance.statPointTxtInBuyPanel.text = "현재 스탯 포인트: " + GameManager.Instance.PlayerSc.StatPoint.ToString();
                break;
            case 13:
                sceneObjs.gameTexts[6].text = GameManager.Instance.PlayerSc.skill.skillExplain;
                break;
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
            hpFillImg = this.sceneObjs.gameImgs[3];
            hpFillCvsg = hpFillImg.GetComponent<CanvasGroup>();
            hpText = hpFillImg.transform.parent.GetChild(1).GetComponent<Text>();
            //infoPanel = this.sceneObjs.gameImgs[2];

            mainCvs = this.sceneObjs.cvses[0];
            touchCvs = this.sceneObjs.cvses[1];
            infoCvs = this.sceneObjs.cvses[2];
            camSlider = this.sceneObjs.camSlider;
            curMenuPanel = this.sceneObjs.ui[1];

            objExplainText = this.sceneObjs.gameTexts[1];
            timerText = this.sceneObjs.gameTexts[2];
            missionCntText = this.sceneObjs.gameTexts[5];

            interactionBtns = new List<InteractionBtn>(this.sceneObjs.itrBtns);
            uiColors = new List<Color>(this.sceneObjs.gameColors);
            menuBtn = this.sceneObjs.gameBtns[0];

            camMove = this.sceneObjs.camMove;
            mainCam = camMove.GetComponent<Camera>();
            timerParent = timerText.transform.parent.gameObject;

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

        npcTalkImgs = FunctionGroup.CreatePoolList(sceneObjs.prefabs[4], sceneObjs.trms[0], 6);
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
        sceneObjs.gameTexts[0].text = GameManager.Instance.savedData.userInfo.money.ToString() + " 골드";  //돈 텍스트 업데이트
    }

    private void Update()
    {
        _Input();
        Timer();
        //deleteValue = uiChangeQueue.Count;
    }

    private void _Input()  //컴용
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

    public void OnSystemPanel(string msg, int fontSize=69)  //시스템 패널 띄우고 메시지까지
    {
        OnClickUIButton(6);
        Text t = sceneObjs.ui[6].transform.GetChild(0).GetComponent<Text>();
        t.text = msg;
        t.fontSize = fontSize;
    }

    public void OnTimer(int time,bool useCnt ,bool off=false)  //타이머 시작 or 종료
    {
        if (off)
        {
            isTimer = false;
            timerParent.SetActive(false);
            missionCntText.gameObject.SetActive(false);

            foreach(Action a in timeOverEvent.GetInvocationList())
            {
                timeOverEvent -= a;
            }
            foreach(Action a in clearEvent.GetInvocationList())
            {
                clearEvent -= a;
            }

            return;
        }

        if (!isTimer)
        {
            runningMission = true;
            isTimer = true;
            remainingTime = time;
            timerParent.SetActive(true);
            missionCntText.gameObject.SetActive(useCnt);
            timerText.text = ((int)remainingTime).ToString();
        }
    }

    private void Timer() //타이머 기능
    {
        if (isTimer)
        {
            remainingTime -= Time.deltaTime;
            timerText.text = ((int)remainingTime).ToString();

            if (remainingTime <= 0)
            {
                timeOverEvent?.Invoke();
                OnTimer(0, true);
            }
        }
    }

    public void TimeAttackMission(bool clear)  //타임어택 미션 클리어 혹은 실패 했을 때
    {
        if (clear) clearEvent?.Invoke();
        else timeOverEvent?.Invoke();

        {
            Text resultText = sceneObjs.gameTexts[clear ? 3 : 4];
            Color c = resultText.color;
            resultText.color = noColor;
            resultText.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            resultText.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(resultText.DOColor(c, 0.3f))
            .Join(resultText.transform.DOScale(Vector3.one, 0.3f))
            .AppendInterval(1)
            .Append(resultText.DOColor(noColor, 0.4f))
            .AppendCallback(() => { resultText.color = c; resultText.gameObject.SetActive(false); runningMission = false; })
            .Play();
        }

        OnTimer(0, true, true);
    }

    public void UpdateCountInMission(float cur, float max)  //미션 중 목표 달성량과 현재 진행도
    {
        missionCntText.text = string.Concat(cur.ToString(), "/", max.ToString());
    }

    public void OnNPCMessage(NPCHPLowMsg msg, Transform target, Vector3 offset)  //NPC가 일정 HP이하일 때 NPC가 대화창 띄우면서 혼자 말하는 UI
    {
        GameObject msgImage = FunctionGroup.GetPoolItem(npcTalkImgs);
        Text msgText = msgImage.transform.GetChild(1).GetComponent<Text>();
        msgText.text = msg.message;
        msgText.fontSize = msg.fontSize;

        CanvasGroup cvg = msgImage.GetComponent<CanvasGroup>();
        cvg.alpha = 0;

        StartCoroutine(GameManager.Instance.FuncHandlerCo(msg.time,()=>cvg.DOFade(1,0.4f),()=>
        {
            msgImage.transform.position = mainCam.WorldToScreenPoint(target.position + offset);
        }, ()=> msgImage.SetActive(false)) );
    }

    public void SaveData()  //주로 옵션에서 설정한 값 저장
    {
        Option op = new Option();
        op.distFromCam = camSlider.value;
        GameManager.Instance.savedData.option = op;
    }
}

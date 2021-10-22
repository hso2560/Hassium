using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class JoystickControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectBackground;
    [SerializeField] private RectTransform rectJoystick;

    public Color bgAfter, joystickAfter;
    private Color bgBefore, joystickBefore;
    private Image bgImg, joystickImg;
    [SerializeField] private Image staminaGauge, skillGauge;
    [SerializeField] private CanvasGroup staminaGaugeCvg;
    [SerializeField] private Color defaultGaugeColor, shortageGaugeColor;
    private bool isShortage = false;  //스태미나 부족 상태
    private bool isStmFull = true; //스태미나 꽉 찬 상태?  

    public Button jumpBtn;
    public Button skillBtn;
    public Button aimBtn;
    public Button attackBtn;
    
    private CanvasGroup jumpBtnCvsGroup;
    [SerializeField] private CanvasGroup runBtnCvg;

    [HideInInspector] public PlayerScript player;

    private float radius;
    private Vector2 dragVec;
    [HideInInspector] public Vector2 dirVec;

    [HideInInspector] public bool isTouch = false;
    [HideInInspector] public bool isRun;
    [HideInInspector] public bool isHoldSkill;
    [HideInInspector] public bool isAimState;
    [HideInInspector] public GameObject crosshair;


    [HideInInspector] public Vector3 PC_MoveDir;

    //public EventTrigger trigger;

    //public EventTrigger.Entry entry1 = new EventTrigger.Entry();
    //public EventTrigger.Entry entry2 = new EventTrigger.Entry();

    private void Awake()
    {
        radius = rectBackground.rect.width * 0.5f;
        jumpBtn.onClick.AddListener(Jump);
        skillBtn.onClick.AddListener(ClickSkill);
        aimBtn.onClick.AddListener(Aim);
        attackBtn.onClick.AddListener(() =>
        {
            player.Attack();
        });

        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
        bgBefore = bgImg.color;
        joystickBefore = joystickImg.color;

        jumpBtnCvsGroup = jumpBtn.GetComponent<CanvasGroup>();
        //skillGauge = skillBtn.transform.GetChild(0).GetComponent<Image>();

        //trigger = skillBtn.GetComponent<EventTrigger>();
        //entry1.eventID = EventTriggerType.PointerDown;
        //entry1.callback = new EventTrigger.TriggerEvent();
        //entry2.eventID = EventTriggerType.PointerUp;
        //entry2.callback = new EventTrigger.TriggerEvent();
    }

    private void Start()
    {
        crosshair = UIManager.Instance.crosshairImg.gameObject;
    }

    #region Move 패드
    public void OnDrag(PointerEventData eventData)
    {
        dragVec = eventData.position - (Vector2)rectBackground.position;
        dragVec = Vector2.ClampMagnitude(dragVec, radius);
        rectJoystick.localPosition = dragVec;

        dirVec = dragVec.normalized;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouch = true;

        dragVec = eventData.position - (Vector2)rectBackground.position;
        dragVec = Vector2.ClampMagnitude(dragVec, radius);
        rectJoystick.localPosition = dragVec;

        dirVec = dragVec.normalized;

        bgImg.color = bgAfter;
        joystickImg.color = joystickAfter;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        rectJoystick.localPosition = Vector3.zero;

        bgImg.color = bgBefore;
        joystickImg.color = joystickBefore;
    }
    #endregion

    private void Update()
    {
        if (player != null)
        {
            CheckJoystickState();
            CheckSkillCool();
        }

        PC_MoveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    #region 버튼 처리
    public void OnPointerDownRunBtn() => isRun = !player.isStamina0;
    public void OnPointerUpRunBtn() => isRun = false;

    private void Jump()
    {
        player.Jump();
        //점프 했을 때 각종 처리 
    }

    private void ClickSkill()
    {
        if (!isHoldSkill)
        {
            player.skill.UseSkill();
        }
    }
    
    public void DownSkill()
    {
        if(isHoldSkill)
        {
            player.skill.UseSkill();
        }
    }

    public void UpSkill()
    {
        if(isHoldSkill)
        {
            player.skill.OffSkill();
        }
    }

    public void Aim()
    { 
        crosshair.SetActive(!crosshair.activeSelf);
    }
    #endregion

    #region Check
    public void CheckJoystickState()
    {
        //점프 버튼
        bool jumpCheck = player.isJumping;
        jumpBtnCvsGroup.alpha = !jumpCheck ? 1 : 0.3f;
        jumpBtnCvsGroup.blocksRaycasts = !jumpCheck ? true : false;
        jumpBtnCvsGroup.interactable = !jumpCheck ? true : false;

        //공격 버튼
        attackBtn.GetComponent<CanvasGroup>().alpha = (player.isJumping || isTouch) ? 0.4f : 1;
    }

    public void CheckRunBtnState(bool zero)
    {
        if (zero) isRun = false;

        runBtnCvg.alpha = zero ? 0.3f : 1;
        runBtnCvg.blocksRaycasts = !zero;
        runBtnCvg.interactable = !zero;
    }

    public void CheckStamina(float curStamina, float maxStamina, float needStaminaMin)
    {
        if (curStamina < maxStamina)
        {
            if (isStmFull)
            {
                staminaGaugeCvg.gameObject.SetActive(true);
                isStmFull = false;
            }
            else if(!staminaGaugeCvg.gameObject.activeSelf)
            {
                staminaGaugeCvg.gameObject.SetActive(true);
            }
        }
        else if(curStamina==maxStamina && !isStmFull)
        {
            staminaGaugeCvg.GetComponent<GameUI>().ResetData();
            isStmFull = true;
        }

        staminaGauge.fillAmount = curStamina / maxStamina;
        staminaGauge.color = curStamina <= needStaminaMin ? shortageGaugeColor : defaultGaugeColor;

        if(curStamina <= needStaminaMin && !isShortage)
        {
            staminaGaugeCvg.DOKill();
            staminaGaugeCvg.DOFade(0.2f, 0.3f).SetLoops(-1,LoopType.Yoyo);
            isShortage = true;
        }

        if(curStamina>needStaminaMin && isShortage)
        {
            isShortage = false;
            staminaGaugeCvg.DOKill();
            staminaGaugeCvg.DOFade(1, 0.3f);
        }
    }

    private void CheckSkillCool()
    {
        if (player.skill.isUsedSkill)
        {
            skillGauge.fillAmount = (player.skill.skillRechargeTime - Time.time) / player.skill.coolTime;
        }
        else
        {
            skillGauge.fillAmount = player.skill.isUsingSkill ? 1 : 0;
        }
    }
    #endregion

    #region 주석
    /*public void ClearSkillBtn()
    {
        skillBtn.onClick.RemoveAllListeners();

        entry1.callback.RemoveAllListeners();
        entry2.callback.RemoveAllListeners();

        if (trigger.triggers.Count > 0)
            trigger.triggers.RemoveRange(0, 2);
    }

    public void SkillBtnTriggerAdd()
    {
        trigger.triggers.Add(entry1);
        trigger.triggers.Add(entry2);
    }*/
    #endregion
}

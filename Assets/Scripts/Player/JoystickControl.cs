using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectBackground;
    [SerializeField] private RectTransform rectJoystick;

    public Color bgAfter, joystickAfter;
    private Color bgBefore, joystickBefore;
    private Image bgImg, joystickImg;
    [SerializeField] private Image staminaGauge;
    [SerializeField] private CanvasGroup staminaGaugeCvg;

    public Button jumpBtn;
    public Button skillBtn;
    public Button aimBtn;
    
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

    //public EventTrigger trigger;

    //public EventTrigger.Entry entry1 = new EventTrigger.Entry();
    //public EventTrigger.Entry entry2 = new EventTrigger.Entry();

    private void Awake()
    {
        radius = rectBackground.rect.width * 0.5f;
        jumpBtn.onClick.AddListener(Jump);
        skillBtn.onClick.AddListener(ClickSkill);
        aimBtn.onClick.AddListener(Aim);

        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
        bgBefore = bgImg.color;
        joystickBefore = joystickImg.color;

        jumpBtnCvsGroup = jumpBtn.GetComponent<CanvasGroup>();

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

    #region Move �е�
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
        }
    }

    #region ��ư ó��
    public void OnPointerDownRunBtn() => isRun = !player.isStamina0;
    public void OnPointerUpRunBtn() => isRun = false;

    private void Jump()
    {
        player.Jump();
        //���� ���� �� ���� ó�� 
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

    public void CheckJoystickState()
    {
        //���� ��ư
        bool jumpCheck = player.isJumping;
        jumpBtnCvsGroup.alpha = !jumpCheck ? 1 : 0.3f;
        jumpBtnCvsGroup.blocksRaycasts = !jumpCheck ? true : false;
        jumpBtnCvsGroup.interactable = !jumpCheck ? true : false;
    }

    public void CheckRunBtnState(bool zero)
    {
        if (zero) isRun = false;

        runBtnCvg.alpha = zero ? 0.3f : 1;
        runBtnCvg.blocksRaycasts = !zero;
        runBtnCvg.interactable = !zero;
    }

    public void CheckStamina(Vector3 dir, float curStamina, float maxStamina)
    {
        if (curStamina < maxStamina)
        {
            staminaGaugeCvg.gameObject.SetActive(true);       
        }
        else
        {
            //false�ϱ����� �ִϸ��̼�
        }

        staminaGauge.fillAmount = curStamina / maxStamina;
    }

    #region �ּ�
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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectBackground;
    [SerializeField] private RectTransform rectJoystick;
    public Button jumpBtn;
    public PlayerScript player;

    private float radius;
    private Vector2 dragVec;
    public Vector2 dirVec;

    public bool isTouch = false;
    public bool isRun;

    private void Awake()
    {
        radius = rectBackground.rect.width * 0.5f;
    }

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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        rectJoystick.localPosition = Vector3.zero;
    }

    
}

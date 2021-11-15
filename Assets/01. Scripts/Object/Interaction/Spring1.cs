using UnityEngine;
using DG.Tweening;

public class Spring1 : MonoBehaviour
{
    [SerializeField] private Spring1Puzzle rule;
    [HideInInspector] public Light pressLight;
    private float startY; //����Ʈ�� ÷ ��ġ�� y

    public float lightMoveY; //����Ʈ�� ���� ������ �� y���� ��ŭ �ø���
    public bool active = true;
    public int[] relevantId;

    private void Awake()
    {
        if(rule==null) rule = transform.parent.GetChild(0).GetComponent<Spring1Puzzle>();
        pressLight = transform.GetChild(0).GetComponent<Light>();
        startY = pressLight.transform.localPosition.y;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(active && collision.transform.CompareTag("Player"))
        {
            rule.OnPressInteraction(relevantId,true);
            pressLight.transform.DOLocalMoveY(lightMoveY,1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (active && collision.transform.CompareTag("Player"))
        {
            rule.OnPressInteraction(relevantId,false);
            pressLight.transform.DOLocalMoveY(startY, 1);
        }
    }
}

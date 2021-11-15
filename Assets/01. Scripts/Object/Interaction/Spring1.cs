using UnityEngine;
using DG.Tweening;

public class Spring1 : MonoBehaviour
{
    [SerializeField] private Spring1Puzzle rule;
    [HideInInspector] public Light pressLight;
    private float startY; //라이트의 첨 위치의 y

    public float lightMoveY; //라이트를 위로 움직일 때 y축을 얼마큼 올릴지
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

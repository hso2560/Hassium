using UnityEngine;

public class Spring1 : MonoBehaviour
{
    private Spring1Puzzle rule;

    public bool active = true;
    public int[] relevantId;

    private void Awake()
    {
        rule = transform.parent.GetChild(0).GetComponent<Spring1Puzzle>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(active && collision.transform.CompareTag("Player"))
        {
            rule.OnPressInteraction(relevantId,true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (active && collision.transform.CompareTag("Player"))
        {
            rule.OnPressInteraction(relevantId,false);
        }
    }
}

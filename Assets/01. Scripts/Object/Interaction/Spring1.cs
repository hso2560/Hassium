using UnityEngine;

public class Spring1 : MonoBehaviour
{
    private Spring1Puzzle rule;

    public bool active = true;
    public int[] relevantId;

    private void Awake()
    {
        //rule = transform.parent.GetChild(0).GetComponent<Spring1Puzzle>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        int l = 1 << collision.gameObject.layer;
        if(active && (l == 1<<6 || l == 1<<16))
        {
            Debug.Log(1);
            //rule.OnPressInteraction(relevantId,true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        int l = 1 << collision.gameObject.layer;
        if (active && (l == 1 << 6 || l == 1 << 16))
        {
            Debug.Log(2);
            //rule.OnPressInteraction(relevantId,false);
        }
    }
}

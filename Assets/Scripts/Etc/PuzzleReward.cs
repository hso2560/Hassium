using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleReward : MonoBehaviour
{
    private static PuzzleReward instance;

    [SerializeField] private NPCAI[] npc;
    [SerializeField] private GameObject[] objects;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public static void RequestReward(short id)
    {
        instance.StartCoroutine(instance.RewardCo(id));
    }

    public IEnumerator RewardCo(short id)
    {
        yield return null;
        switch (id)
        {
            case 10:
                npc[0].info.talkId++;
                //오브젝트 위치 저장
                //오브젝트 움직임
                break;

            default:
                break;
        }
    }
}

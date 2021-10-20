using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PuzzleReward : MonoBehaviour
{
    private static PuzzleReward instance;

    [SerializeField] private NPCAI[] npc;
    [SerializeField] private GameObject[] objects;
    [SerializeField] private Vector3[] vectors;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public static void RequestReward(short id, bool running=true)
    {
        instance.StartCoroutine(instance.RewardCo(id,running));
    }

    public IEnumerator RewardCo(short id, bool running)
    {
        yield return null;
        switch (id)
        {
            case 10:
                if(running)
                   npc[0].info.talkId++;
                objects[0].SetActive(true);
                objects[0].transform.DOMove(Vector3.zero, running ? 4f : 0);
                break;

            default:
                objects[1].SetActive(true);
                EffectManager.Instance.OnEffect(EffectType.APPEARANCE, objects[1].transform.position, 0.7f);
                break;
        }
    }
}

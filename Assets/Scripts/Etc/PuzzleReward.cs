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

    public static void RequestReward(int id, bool running=true)
    {
        instance.StartCoroutine(instance.RewardCo(id,running));
    }

    public IEnumerator RewardCo(int id, bool running)
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

            case 12:
                RewardChest(1);
                break;

            case 16:
                RewardChest(2);
                break;
        }
    }

    public void RewardChest(int index)
    {
        PoolManager.GetItem<SystemTxt>().OnText("보물상자가 나타났습니다!");
        objects[index].SetActive(true);
        EffectManager.Instance.OnEffect(EffectType.APPEARANCE, objects[index].transform.position, 0.7f);
    }
}

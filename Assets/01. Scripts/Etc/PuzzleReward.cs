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

    public static void RequestReward(int id, bool running=true) //다른 곳에선 이걸로 코루틴 호출
    {
        instance.StartCoroutine(instance.RewardCo(id,running));
    }

    public IEnumerator RewardCo(int id, bool running) //뭔가 딜레이 필요할 수도 있으니 코루틴으로 함
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
                npc[1].info.talkId = 1;
                break;

            case 20:
                RewardChest(3);
                break;
            case 24:
                if (running)
                {
                    RewardChest(4);
                    objects[6].SetActive(true);
                }
                objects[5].SetActive(true);
                objects[5].transform.DOMove(vectors[0], running ? 3: 0);
                break;
            case 28:
                RewardChest(7);
                if (running) npc[2].info.talkId = 1;
                break;
            case 30:
                RewardChest(8);
                break;
            case 32:
                RewardChest(10);
                break;
            case 34:
                RewardChest(9);
                break;
        }
    }

    public void RewardChest(int index) //상자 꺼내짐. 있으면 메시지
    {
        if (objects[index].activeSelf)
        {
            PoolManager.GetItem<SystemTxt>().OnText("보물상자가 이미 존재합니다.");
            return;
        }

        PoolManager.GetItem<SystemTxt>().OnText("보물상자가 나타났습니다!");
        objects[index].SetActive(true);
        EffectManager.Instance.OnEffect(EffectType.APPEARANCE, objects[index].transform.position, 0.7f);
    }
}

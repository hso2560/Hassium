using System.Collections.Generic;
using UnityEngine;

public class Spring1Puzzle : MonoBehaviour, IReward
{
    public int id;
    public bool IsClear { get; set; }
    [SerializeField] private int saveActiveStateId;

    public List<SpringObject> springObjs = new List<SpringObject>();

    public float[] springMaxDistRangeForClear; //길이: 2

    private float checkTime;

    [SerializeField] NPCAI npc;
    public string npcFightingClearMsg;
    public int chestId;

    private void Start()
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            IsClear = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if(IsClear)
        {
            foreach (Spring1 s in GetComponentsInChildren<Spring1>()) s.active = false;
            float d = (springMaxDistRangeForClear[0] + springMaxDistRangeForClear[1]) * 0.5f;

            springObjs.ForEach(x => x.spring.maxDistance = d);
        }
        else
        {
            springObjs.ForEach(x => x.spring.maxDistance = x.max_springMaxDist);
        }
    }

    public void OnPressInteraction(int[] idx, bool press)
    {
        if (!IsClear)
        {
            for (int i = 0; i < springObjs.Count; ++i)
            {
                springObjs[i].IsPressing = false;
            }

            if(press)
            {
                for(int i =0; i<idx.Length; ++i)
                {
                    springObjs[idx[i]].IsPressing = true;
                }
            }
        }
    }
    private void Update()
    {
        if(!IsClear)
        {
            if(Time.time > checkTime)
            {
                checkTime = Time.time + 0.15f;

                for(int j=0; j<springObjs.Count; ++j)
                {
                    SpringObject sp = springObjs[j];

                    float m_dist = sp.spring.maxDistance + (sp.IsPressing ? -sp.interval : sp.interval);
                    sp.spring.maxDistance = Mathf.Clamp(m_dist, sp.min_springMaxDist, sp.max_springMaxDist);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if(!IsClear)
        {
            for (int k = 0; k < springObjs.Count; ++k)
            {

            }
        }
    }

    public void GetReward()
    {
        if (!GameManager.Instance.IsContainChest(chestId))
        {
            PuzzleReward.RequestReward(id);
        }
        else
        {
            PoolManager.GetItem<SystemTxt>().OnText("이곳의 보물상자는 이미 가져갔습니다.");
        }
    }
}

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

    private bool inRange = false;
    private float stayTime = 0;
    public float targetTime = 2f;

    private void Awake()
    {
        foreach(SpringObject s in springObjs) //damper spring massscale 등은 그냥 옵젝에서 바꿔준다
        {
            s.spring.connectedAnchor = s.connectedTr.position;
            s.spring.minDistance = s.min_springMaxDist - 0.05f;
            s.spring.maxDistance = s.max_springMaxDist;
        }
    }

    private void Start()
    {
        if (GameManager.Instance.ContainKeyActiveId(saveActiveStateId))
        {
            IsClear = GameManager.Instance.savedData.objActiveInfo[saveActiveStateId];
        }
        if(IsClear)
        {
            foreach (Spring1 s in transform.parent.GetComponentsInChildren<Spring1>())
            {
                s.active = false;
                s.pressLight.gameObject.SetActive(false);
            }
            //float d = (springMaxDistRangeForClear[0] + springMaxDistRangeForClear[1]) * 0.5f;

            //springObjs.ForEach(x => x.spring.maxDistance = d);
            //springObjs.ForEach(sp => sp.line.SetPosition(1, sp.line.transform.InverseTransformPoint(sp.connectedTr.position)));
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
            if (Time.time > checkTime)
            {
                checkTime = Time.time + 0.15f;

                for(int j=0; j<springObjs.Count; ++j)
                {
                    SpringObject sp = springObjs[j];

                    float m_dist = sp.spring.maxDistance + (sp.IsPressing ? -sp.interval : sp.interval*0.5f);
                    sp.spring.maxDistance = Mathf.Clamp(m_dist, sp.min_springMaxDist, sp.max_springMaxDist);

                    if (sp.spring.maxDistance < springMaxDistRangeForClear[1]) sp.bComplete = true;
                }

                CheckInRange();
            }
            
            if(inRange)
            {
                stayTime += Time.deltaTime;
                if (stayTime > targetTime)
                {
                    PoolManager.GetItem<SystemTxt>().OnText("<color=green>성공!</color>",3,61);
                    foreach (Spring1 s in transform.parent.GetComponentsInChildren<Spring1>())
                    {
                        s.active = false;
                        s.pressLight.gameObject.SetActive(false);
                    }

                    if (npc != null) npc.info.talkId = 1;

                    if (npc != null && !npc.info.dead && (npc.info.isFighting || npc.info.bRunaway))
                        PoolManager.GetItem<SystemTxt>().OnText(npcFightingClearMsg);
                    else
                        GetReward();

                    GameManager.Instance.savedData.objActiveInfo[saveActiveStateId] = true;
                    IsClear = true;
                }
            }
        }

        springObjs.ForEach(sp => sp.line.SetPosition(1, sp.line.transform.InverseTransformPoint(sp.connectedTr.position)));
    }

    private void CheckInRange()
    {
        for(int i=0; i<springObjs.Count; i++)
        {
            if (!springObjs[i].bComplete)
            {
                inRange = false;
                stayTime = 0;
                return;
            }
        }

        inRange = true;
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

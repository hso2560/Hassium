using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>, ISceneDataLoad
{
    public List<Skill> playerSkills = new List<Skill>();
    //private TimeSpan ts;
    private WaitForSeconds ws = new WaitForSeconds(0.5f);

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    private void Start()
    {
        for(int n=0; n<GameManager.Instance.playerList.Count; ++n)
        {
            playerSkills.Add(GameManager.Instance.playerList[n].skill);
        }

        /*foreach(ObjData o in FindObjectsOfType<ObjData>())  //확인용
        {
            if(o.saveActiveStateId>-1)
               Debug.Log(o.saveActiveStateId + " " + o.gameObject.name);
        }*/
    }

    private IEnumerator SkillUseManage()  //사용중인 스킬 체크하고 시간지나면 쿨타임 시작
    {
        int i;

        while(true)
        {
            yield return ws;

            for(i=0; i<playerSkills.Count; i++)
            {
                if(playerSkills[i].isUsingSkill)
                {
                    if(playerSkills[i].skillOffTime<Time.time)
                    {
                        playerSkills[i].OffSkill();
                    }
                }
            }
        }
    }

    private IEnumerator SkillCoolManage()  //쿨타임 걸린 스킬 체크하고 시간 지나면 다시 사용 가능 상태로
    {
        int j;

        while (true)
        {
            yield return ws;

            for(j=0; j<playerSkills.Count; j++)
            {
                if(playerSkills[j].isUsedSkill)
                {
                    if(playerSkills[j].skillRechargeTime<Time.time)
                    {
                        playerSkills[j].isUsedSkill = false;
                    }
                }
            }
        }
    }

    public void ManagerDataLoad(GameObject sceneObjs)
    {
        SkillManager[] managers = FindObjectsOfType<SkillManager>();
        if (managers.Length > 1) Destroy(gameObject);

        this.sceneObjs = sceneObjs.GetComponent<SceneObjects>();

        StopAllCoroutines();

        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            StartCoroutine(SkillUseManage());
            StartCoroutine(SkillCoolManage());
        }

        isReady = true;
    }
}

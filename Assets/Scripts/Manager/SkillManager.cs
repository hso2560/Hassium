using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>, ISceneDataLoad
{
    public List<Skill> playerSkills = new List<Skill>();
    //private TimeSpan ts;
    private WaitForSeconds ws = new WaitForSeconds(0.5f);

    private bool isCoWorking = false;
    private IEnumerator skillCo1, skillCo2;

    public bool GetReadyState { get { return isReady; } set { isReady = value; } }

    private IEnumerator SkillUseManage()
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

    private IEnumerator SkillCoolManage()
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

        if (this.sceneObjs.ScType == SceneType.MAIN)
        {
            if (!isCoWorking)
            {
                skillCo1 = SkillUseManage();
                skillCo2 = SkillCoolManage();

                StartCoroutine(skillCo1);
                StartCoroutine(skillCo2);

                isCoWorking = true;
            }
        }

        isReady = true;
    }
}

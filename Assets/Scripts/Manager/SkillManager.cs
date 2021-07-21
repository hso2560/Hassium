using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public List<Skill> playerSkills = new List<Skill>();
    //private TimeSpan ts;
    private WaitForSeconds ws = new WaitForSeconds(1);
    private int i, j;

    private void Start()
    {
        //StartCoroutine(SkillUseManage());
        //StartCoroutine(SkillCoolManage());
    }

    private IEnumerator SkillUseManage()
    {
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
        while(true)
        {
            yield return ws;

            for(j=0; j<playerSkills.Count; j++)
            {
                if(playerSkills[i].isUsedSkill)
                {
                    if(playerSkills[i].skillRechargeTime<Time.time)
                    {
                        playerSkills[i].isUsedSkill = false;
                    }
                }
            }
        }
    }
}

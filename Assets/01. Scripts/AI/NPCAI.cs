using System.Collections.Generic;
using UnityEngine;

public class NPCAI : ObjData
{
    public NPCInfo info;
    public GameObject puzzleRuleObj;
    public EnemyBase enemy;

    public ItemData[] deathItems; //������ ������ �۵� ������
    public int[] dropedCnt; //������ ������ ����

    [SerializeField] private bool canInactive;
    [SerializeField] private int index;

    private void Start()
    {
        objName = info.name;

        if(GameManager.Instance.savedData.npcInfo.objActiveKeys.Contains(info.id))
        {
            info = GameManager.Instance.savedData.npcInfo[info.id];
        }
        else
        {
            GameManager.Instance.savedData.npcInfo[info.id] = info;
        }

        if (info.dead)  //���� �ִ� ������ϴϱ�
        {
            enemy.CurrentHp = 0;
            enemy.isDie = true;
            enemy.enemyState = EnemyState.DIE;
            gameObject.SetActive(false);
        }

        if(canInactive)
        {
            GameManager.Instance.SaveObjActiveInfo(index, true); 
        }

        if (info.bRunaway || info.isFighting) //���� ���� ���� �̾��
        {
            active = false;
            enemy.StartEnemy();
        }
    }

    public override void Interaction()
    {
        base.Interaction();

        TalkManager.Instance.StartTalk(info);
    }

    public void Death()
    {
        puzzleRuleObj.GetComponent<IReward>().GetReward();
        info.bRunaway = false;
        info.isFighting = false;
        info.dead = true;
        for(int i=0; i<deathItems.Length; i++)
        {
            Item item = PoolManager.GetItem<Item>();
            item.itemData = deathItems[i];
            item.droppedCount = dropedCnt[i];
            item.index = -1;
            item.objName = string.Concat(item.itemData.name, "��", item.droppedCount);
            item.transform.position = enemy.center.position;
            item.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0f,0.25f),1, Random.Range(0f, 0.25f))*Random.Range(30f,45f), ForceMode.Impulse);
        }
        GameManager.Instance.KillNPC();
    }
}

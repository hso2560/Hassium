using System.Collections.Generic;
using UnityEngine;

public class FunctionGroup
{
    public static Vector3 GetRandomVector(Vector3 a, Vector3 b) => new Vector3(Random.Range(a.x, b.x), Random.Range(a.y, b.y), Random.Range(a.z, b.z));

    public static List<int> GetRandomList(int length, int count)
    {
        List<int> randomList = new List<int>();

        while (count > 0)
        {
            int random;
            do
            {
                random = Random.Range(0, length);
            } while (randomList.Contains(random));
            count--;
        }

        return randomList;
    }

    #region ���� ��ũ��Ʈ ���̱� �ָ��� Ǯ�� �ʿ��� �������� �ϴ� �̷��� ó����
    public static List<GameObject> CreatePoolList(GameObject prefab, Transform parent, int count)
    {
        List<GameObject> poolList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject o = GameObject.Instantiate(prefab, parent);
            poolList.Add(o);
            o.SetActive(false);
        }

        return poolList;
    }

    public static GameObject GetPoolItem(List<GameObject> poolList)
    {
        GameObject o = poolList.Find(x => !x.activeSelf);
        if (o == null)
        {
            poolList[0].SetActive(false);
            o = poolList[0];
        }
        o.SetActive(true);

        return o;
    }
    #endregion
}

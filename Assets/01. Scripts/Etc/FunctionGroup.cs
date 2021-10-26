using System.Collections.Generic;
using UnityEngine;

public class FunctionGroup
{
    public const float defCriteria = 250f;
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
            randomList.Add(random);
            count--;
        }

        return randomList;
    }

    public static Vector3 PositionLimit(Vector3 targetVec, Vector3 minVec, Vector3 maxVec)
    {
        float X = Mathf.Clamp(targetVec.x, minVec.x, maxVec.x);
        float Y = Mathf.Clamp(targetVec.y, minVec.y, maxVec.y);
        float Z = Mathf.Clamp(targetVec.z, minVec.z, maxVec.z);

        return new Vector3(X, Y, Z);
    }

    public static Vector3 GetRandomDir() => new Vector3(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

    public static bool IsContainValue<T>(T[] arr, T value) where T : struct
    {
        for(int i=0; i<arr.Length; i++)
        {
            //if (arr[i] == value) return true;
            if (EqualityComparer<T>.Default.Equals(arr[i], value)) return true;
        }
        return false;
    }

    public static int GetDamageAmount(int damage, int def) => Mathf.Clamp((int)(damage - ((float)damage * def / defCriteria)), 0, damage);
    
    public static void Look(Transform target, Transform center, bool yZero = true)
    {
        Vector3 lookPos = target.position - center.position;
        if (yZero) lookPos.y = 0;
        lookPos.Normalize();
        center.rotation = Quaternion.LookRotation(lookPos);
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
        if (o == null)  //��������� �ȿ��� �Ұ�
        {
            poolList[0].SetActive(false);
            o = poolList[0];
        }
        o.SetActive(true);

        return o;
    }
    #endregion
}
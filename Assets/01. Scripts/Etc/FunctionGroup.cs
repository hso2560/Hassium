using System.Collections.Generic;
using UnityEngine;

public class FunctionGroup
{
    public const float defCriteria = 250f;
    public static Vector3 GetRandomVector(Vector3 a, Vector3 b) => new Vector3(Random.Range(a.x, b.x), Random.Range(a.y, b.y), Random.Range(a.z, b.z)); //두 벡터 사이의 랜덤값

    public static List<int> GetRandomList(int length, int count) //배열에서 요소들을 섞음
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

    public static Vector3 PositionLimit(Vector3 targetVec, Vector3 minVec, Vector3 maxVec) //위치 리미트
    {
        float X = Mathf.Clamp(targetVec.x, minVec.x, maxVec.x);
        float Y = Mathf.Clamp(targetVec.y, minVec.y, maxVec.y);
        float Z = Mathf.Clamp(targetVec.z, minVec.z, maxVec.z);

        return new Vector3(X, Y, Z);
    }

    public static Vector3 GetRandomDir() => new Vector3(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); //랜덤 벡터 방향 뱉음

    public static bool IsContainValue<T>(T[] arr, T value) where T : struct //배열에서 주어진 값과 같은게 있는지 찾는다
    {
        for(int i=0; i<arr.Length; i++)
        {
            //if (arr[i] == value) return true;
            if (EqualityComparer<T>.Default.Equals(arr[i], value)) return true;
        }
        return false;
    }

    public static int GetDamageAmount(int damage, int def) => Mathf.Clamp((int)(damage - ((float)damage * def / defCriteria)), 0, damage);  //최종데미지 계산식
    
    public static void Look(Transform target, Transform center, bool yZero = true) //어떤 옵젝이 어떤 방향을 바라보게
    {
        Vector3 lookPos = target.position - center.position;
        if (yZero) lookPos.y = 0;
        lookPos.Normalize();
        center.rotation = Quaternion.LookRotation(lookPos);
    }

    /*public static Color GetAlphaColor(Color currentColor, float targetAlpha) //색의 투명도만 바꿀 때 씀
    {
        currentColor.a = targetAlpha;
        return currentColor;
    }*/

    #region 따로 스크립트 붙이기 애매한 풀링 필요한 옵젝들은 일단 이렇게 처리함
    public static List<GameObject> CreatePoolList(GameObject prefab, Transform parent, int count) //풀 생성
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

    public static GameObject GetPoolItem(List<GameObject> poolList) //풀에서 옵젝 받아온다
    {
        GameObject o = poolList.Find(x => !x.activeSelf);
        if (o == null)  //여기까지는 안오게 할거
        {
            poolList[0].SetActive(false);
            o = poolList[0];
        }
        o.SetActive(true);

        return o;
    }
    #endregion
}

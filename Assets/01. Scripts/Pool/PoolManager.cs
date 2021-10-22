using System.Collections.Generic;
using UnityEngine;
using System;

public class PoolManager
{
    private static Dictionary<string, IPool> poolDic = new Dictionary<string, IPool>();

    public static void CreatePool<T>(GameObject prefab, Transform parent, int count) where T : MonoBehaviour
    {
        Type t = typeof(T);

        if (!poolDic.ContainsKey(t.ToString()))
        {
            ObjPool<T> pool = new ObjPool<T>(prefab, parent, count);
            poolDic.Add(t.ToString(), pool);
        }
    }

    public static T GetItem<T>() where T : MonoBehaviour
    {
        Type t = typeof(T);
        ObjPool<T> pool = (ObjPool<T>)poolDic[t.ToString()];
        return pool.GetOrCreate();
    }

    public static void ClearItem<T>() where T : MonoBehaviour
    {
        Type t = typeof(T);
        //ObjPool<T> p = (ObjPool<T>)poolDic[t.ToString()];
        //p.ClearItem();
        if(poolDic.ContainsKey(t.ToString()))
           poolDic.Remove(t.ToString());
    }

    public static void ClearAllItem()
    {
        poolDic.Clear();
    }
}

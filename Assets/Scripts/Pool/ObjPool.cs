using System.Collections.Generic;
using UnityEngine;

public class ObjPool<T> : IPool where T : MonoBehaviour
{
    private Queue<T> queue;
    private GameObject prefab;
    private Transform parent;

    public ObjPool(GameObject prefab, Transform parent, int count)
    {
        this.prefab = prefab;
        this.parent = parent;
        queue = new Queue<T>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            T t = obj.GetComponent<T>();
            obj.SetActive(false);
            queue.Enqueue(t);
        }
    }

    public T GetOrCreate()
    {
        T t = queue.Peek();
        if (t.gameObject.activeSelf)
        {
            GameObject o = GameObject.Instantiate(prefab, parent);
            t = o.GetComponent<T>();
        }
        else
        {
            t = queue.Dequeue();
            t.gameObject.SetActive(true);
        }
        queue.Enqueue(t);
        return t;
    }

    /*public void ClearItem()
    {
        queue.Clear();
    }*/
}

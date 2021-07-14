using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance = null;
    private static object lockObj = new object();

    public static T Instance
    {
        get
        {
            lock(lockObj)
            {
                if(instance==null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;
                    
                    if(instance==null)
                    {
                        instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
                        //instance = new GameObject(typeof(T).ToString(), typeof(T)).AddComponent<T>();  이렇게 하면 T 스크립트가 오브젝트에 두 개 붙음
                    }
                }
                return instance; 
            }
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        T[] ts = FindObjectsOfType<T>();
        if (ts.Length > 1) Destroy(gameObject);
    }
}

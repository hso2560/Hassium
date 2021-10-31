using UnityEngine;

//DontDestroy해두고 씬 이동하면 파괴되지 않고 유지되므로 아무것도 안 건든다면 씬이 새로 로드되고 Awake, Start, OnEnable 호출 X
public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance = null;
    private static object lockObj = new object();

    [HideInInspector] public SceneObjects sceneObjs;
    [HideInInspector] public bool isReady = false;

    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;

                    if (instance == null)
                    {
                        instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
                        //instance = new GameObject(typeof(T).ToString(), typeof(T)).AddComponent<T>();  이렇게 하면 T 스크립트가 오브젝트에 두 개 붙음
                    }

                    DontDestroyOnLoad(instance.gameObject);  //이거땜에 종료 시 Some objects where not cleaned up이 뜸. 딱히 이걸로 인한 버그는 안나옴
                }
                return instance;
            }
        }
    }
}

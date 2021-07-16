using UnityEngine;

//DontDestroy�صΰ� �� �̵��ϸ� �ı����� �ʰ� �����ǹǷ� �ƹ��͵� �� �ǵ�ٸ� ���� ���� �ε�ǰ� Awake, Start, OnEnable ȣ�� X
public class MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance = null;
    private static object lockObj = new object();

    public SceneObjects sceneObjs;

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
                        //instance = new GameObject(typeof(T).ToString(), typeof(T)).AddComponent<T>();  �̷��� �ϸ� T ��ũ��Ʈ�� ������Ʈ�� �� �� ����
                    }

                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance; 
            }
        }
    }

}

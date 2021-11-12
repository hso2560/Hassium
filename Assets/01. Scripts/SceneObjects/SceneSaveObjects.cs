using System.Collections.Generic;
using UnityEngine;

public class SceneSaveObjects : MonoBehaviour
{
    public List<GameObject> objs;
    public ObjData[] objDatas;  //��ǻ� �̰Ŵ� �Ⱦ��� ��

    public bool visibleCursor = true;
    private bool isCursor = false;

    private void Awake()
    {
        if (!visibleCursor) Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!visibleCursor)
        {
            if(Input.GetKeyDown(KeyCode.LeftAlt))
            {
                Cursor.lockState = !isCursor ? CursorLockMode.Confined : CursorLockMode.Locked;
                isCursor = !isCursor;
            }
        }
    }

    /*public void QuitGame()
    {
        //�����ϱ����� �� ���ٰ� ������ ���⼭ ���ְ� (�׸Ŵ����� ���� �̺�Ʈ ȣ�����ִ���)

        Application.Quit();
    }*/
}

using System.Collections.Generic;
using UnityEngine;

public class SceneSaveObjects : MonoBehaviour
{
    public List<GameObject> objs;
    public ObjData[] objDatas;  //사실상 이거는 안쓰는 듯

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
        //종료하기전에 뭐 해줄거 있으면 여기서 해주고 (겜매니저의 종료 이벤트 호출해주던가)

        Application.Quit();
    }*/
}

using System.Collections.Generic;
using UnityEngine;

public class SceneSaveObjects : MonoBehaviour
{
    public GameObject[] objs;
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
}

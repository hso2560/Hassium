using System.Collections.Generic;
using UnityEngine;

public class ColumnAPuzzle : MonoBehaviour
{
    public List<ColumnA> columns;

    public float maxHeight, minHeight;
    public float ratio;
    public float moveTime;

    private bool isMove = false;

    private void Awake()
    {
        columns = new List<ColumnA>(GetComponentsInChildren<ColumnA>());
    }

    public void Move()
    {
        if (isMove) return;

        isMove = true;

        columns.ForEach(x => x.active = false);
        Invoke("ActiveCol", moveTime + 0.3f);
    }

    private void ActiveCol()
    {
        columns.ForEach(x => x.active = true); //만약 퍼즐 풀었다면 이거 하면 안됨
    }
}

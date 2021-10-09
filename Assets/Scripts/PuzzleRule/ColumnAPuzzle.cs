using System.Collections.Generic;
using UnityEngine;

public class ColumnAPuzzle : MonoBehaviour
{
    public List<ColumnA> columns;

    public float maxHeight, minHeight;
    public float ratio;
    public float moveTime;

    public int[] objIndex;
    public string Complete;

    private bool isMove = false;
    public bool IsMove { get { return isMove; } }
    ColumnA representativeCol;

    private void Awake()
    {
        columns = new List<ColumnA>(GetComponentsInChildren<ColumnA>());
        columns[0].Representative = true;
        representativeCol = columns[0];
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
        isMove = false;
        float h = columns[0].transform.localScale.y;
        for(int i=1; i<columns.Count; i++)
        {
            if (columns[i].transform.localScale.y != h)
            {
                columns.ForEach(x => x.active = true);
                return;
            }
        }

        representativeCol.Save();
        
    }

    public void AllColActiveState(bool active)
    {
        columns.ForEach(x => x.active = active);

        if (!active)
        {
            Transform tr = representativeCol.transform;
            for(int i=1; i<columns.Count; ++i)
            {
                //columns[i].transform.position = tr.position;
                //columns[i].transform.rotation = tr.rotation;
                columns[i].transform.localScale = tr.localScale;
                columns[i].mesh.material = representativeCol.mesh.material;
            }
        }
    }

    public void AllMatChange(Material mat)
    {
        for(int i=0; i<columns.Count; i++)
        {
            columns[i].mesh.material = mat;
        }
    }
}

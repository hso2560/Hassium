using UnityEngine;
using System.Collections.Generic;

public class RigidMagnet : MonoBehaviour
{
    public float magnetForce = 100;
    private List<Rigidbody> caughtRigids = new List<Rigidbody>();
    private List<Rigidbody> removedRigids = new List<Rigidbody>();
    [SerializeField] private int catchMaxCount = 7;

    public List<GameObject> attachObjs;
    private List<PRS> prsList = new List<PRS>();

    private void Awake()
    {
        for (int i = 0; i <attachObjs.Count; i++)
        {
            prsList.Add(new PRS(attachObjs[i].transform.position, attachObjs[i].transform.rotation));
        }
    }

    private void FixedUpdate() //자석효과
    {
        for(int i=0; i<caughtRigids.Count; i++)
        {
            caughtRigids[i].velocity = (transform.position - (caughtRigids[i].transform.position + caughtRigids[i].centerOfMass)) * magnetForce * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Magnet"))
        {
            Rigidbody r = other.GetComponent<Rigidbody>();
            if (caughtRigids.Count<catchMaxCount && !caughtRigids.Contains(r) && !removedRigids.Contains(r)) //일정 개수 이하로 붙었고 이미 붙은거 아니고 붙은거 끝난게 아닐 경우
            {
                r.isKinematic = false;
                caughtRigids.Add(r);
            }
        }
    }

    public void RemoveCaughtObj() //자석에 붙은것들 없애줌
    {
        for (int i = 0; i < caughtRigids.Count; i++)
        {
            removedRigids.Add(caughtRigids[i]);
            caughtRigids[i].GetComponent<KinematicObj>().colDisable = true;
        }
        caughtRigids.Clear();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Magnet"))
        {
            Rigidbody r = other.GetComponent<Rigidbody>();
            if (caughtRigids.Contains(r))
            {
                caughtRigids.Remove(r);
            }
        }
    }

    public void ResetData()
    {
        caughtRigids.Clear();
        removedRigids.Clear();
        for (int i = 0; i < attachObjs.Count; i++)
        {
            attachObjs[i].GetComponent<KinematicObj>().colDisable = false;
            attachObjs[i].GetComponent<Rigidbody>().isKinematic = false;
            attachObjs[i].transform.position = prsList[i].position;
            attachObjs[i].transform.rotation = prsList[i].rotation;
            attachObjs[i].SetActive(true);
        }
    }

    public bool IsClear() => removedRigids.Count == attachObjs.Count;

    
}

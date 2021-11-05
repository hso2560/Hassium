using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraMove : MonoBehaviour
{
    //[SerializeField] Vector3 defaultCamMinPos, defaultCamMaxPos;  // x: -5.6 y: -50 z: 31.5  x:12.6 y: 100 z:49
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform rotTarget;
    [HideInInspector] public PlayerScript player;
    public JoystickControl joystick;
    //public Vector3 camMinPos, camMaxPos;

    public float xSpeed = 220f, ySpeed = 100f;
    [HideInInspector] public float x, y;
    public float yMinLimit = -20f, yMaxLimit = 80f;

    public Vector3 Offset;
    public float zDistMin = -1f, zDistMax = -6f;

    public float defaultCamDist;

    private Vector3 position;
    private Quaternion rotation;

    private int rotFingerId;
    private float screenWidth, screenHeight;
    private Vector3 firstPoint, secondPoint;

    public bool isPCModeRotateCam = false;

    #region �޽� ���� ����� �� �ʿ�
    //private RaycastHit[] hitArr;
    //private float trpTime;
    //private List<GameObject> transparentList = new List<GameObject>();
    //public LayerMask whatIsWall;

    //private List<GameObject> tempList = new List<GameObject>();
    #endregion

    #region ������Ʈ ������ ���� ����� �� �ʿ�

    private RaycastHit hit;
    public LayerMask cullingObj;
    public float fixDist = 1f;

    #endregion

    private float ClampAngle(float angle, float min, float max)  //ī�޶� �ޱ� ����
    {
        if (angle < -360)
            angle += 360;

        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
     
    private void Start()  
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        screenWidth = Screen.width;
        screenHeight = Screen.height;

        rotFingerId = -1;
    }

    private void Update()
    {
        if (target != null)
        {
            CamMove();
            PlayerRotation();
            //transform.position = FunctionGroup.PositionLimit(transform.position, camMinPos, camMaxPos);
            //ObjectTransparency();  //�޽��� ���� ���
            FrontObject(); //������Ʈ ������ ���� ���
            //Debug.DrawRay(transform.position-transform.forward*0.5f, (target.position - transform.position).normalized * Vector3.Distance(transform.position, target.position), Color.blue);
        }
    }


    private void CamMove()  
    {
        position = rotation * Offset + target.position;
        transform.position = position;

        for(int i=0; i<Input.touchCount; i++)  //��ġ�� ��� �հ��� �˻�
        {
            Touch t = Input.GetTouch(i);

            if (EventSystem.current.IsPointerOverGameObject(t.fingerId) && t.fingerId!=rotFingerId) continue;
             
            switch (t.phase)
            {
                case TouchPhase.Began:  //��ġ ����
                    if (rotFingerId == -1)
                    {
                        rotFingerId = t.fingerId;
                        firstPoint = t.position;
                    }
                    break;

                case TouchPhase.Moved:  //��ġ�ϰ� �����̴� ��
                    if (t.fingerId == rotFingerId)
                    {
                        Move(t,true);
                    }
                    break;

                case TouchPhase.Stationary:  //��ġ�س��� ������ �ִ���
                    if(t.fingerId==rotFingerId)
                    {
                        Move(t,false);
                    }
                    break;

                case TouchPhase.Ended:  //��ġ ��
                    if(t.fingerId==rotFingerId)
                    {
                        rotFingerId = -1;
                    }
                    break;

                case TouchPhase.Canceled:  //��ġ ���
                    if (t.fingerId == rotFingerId)
                    {
                        rotFingerId = -1;
                    } 
                    break;
            }
        }

        if (isPCModeRotateCam)  //PC�� �� �� ī�޶� ������ ó��
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.015f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.015f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
        
            rotation = Quaternion.Euler(y, x, 0);
            position = rotation * Offset + target.position;

            transform.position = position;
            transform.rotation = rotation;
        }
    }

    private void Move(Touch t, bool isFingerMoved=true)  //���⼭ ������ �����̰� ȸ��
    {
        if (isFingerMoved)
        {
            secondPoint = t.position;
            x += (secondPoint.x - firstPoint.x) * xSpeed / screenWidth;
            y -= (secondPoint.y - firstPoint.y) * ySpeed / screenHeight;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            firstPoint = secondPoint;
        }

        rotation = Quaternion.Euler(y, x, 0);
        position = rotation * Offset + target.position;

        transform.position = position;
        transform.rotation = rotation;

        /*x += Input.GetAxis("Mouse X") * xSpeed * 0.015f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.015f;
        y = ClampAngle(y, yMinLimit, yMaxLimit);
        
        rotation = Quaternion.Euler(y, x, 0);
        position = rotation * Offset + target.position;

        transform.position = position;
        transform.rotation = rotation;*/
    }

    private void PlayerRotation()  //�÷��̾ ȸ����Ŵ
    {
        if ((joystick.PC_MoveDir!=Vector3.zero || joystick.isTouch) && !player.isJumping)
        {
            rotTarget.rotation = Quaternion.Slerp(rotTarget.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * player.rotateSpeed);
        }
    }

    /*public void ResetRange()  //ī�޶� ��ġ ���� �ʱ�ȭ
    {
        camMinPos = defaultCamMinPos;
        camMaxPos = defaultCamMaxPos;
    }*/

    public void Setting(Transform target, Transform rotTarget)
    {
        this.target = target;
        this.rotTarget = rotTarget;
    }

    private void FrontObject()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position-transform.forward*0.5f,dir , out hit, Vector3.Distance(transform.position, target.position),cullingObj))
        {
            transform.position = hit.point + dir * fixDist;
        }
    }

    /*private void ObjectTransparency()  //ī�޶�� �÷��̾� ������ ������Ʈ�� �޽��� ���ش�
    {
        if(trpTime < Time.time)
        {
            trpTime = Time.time + 0.1f;

            float dist = Vector3.Distance(transform.position, target.position);
            hitArr = Physics.RaycastAll(transform.position, (target.position - transform.position).normalized, dist, whatIsWall);

            int i, j;
            tempList.Clear();

            transparentList.ForEach(x =>
            {
                bool e = false;
                for (i = 0; i < hitArr.Length; i++)
                {
                    if (x == hitArr[i].transform.gameObject)
                    {
                        e = true;
                        break;
                    }
                }
                if (!e)
                {
                    MeshRenderer[] mrArr = x.GetComponentsInChildren<MeshRenderer>();
                    for (j = 0; j < mrArr.Length; j++)
                    {
                        mrArr[j].enabled = true;
                    }
                    //transparentList.Remove(x);  //foreach������ �ִµ� ������ �ع����ϱ� Invalid��.(�۵��� �� ��) (UIManager�� ��ȣ�ۿ� ��ư ���ִ� �κп����� ����)
                    tempList.Add(x);
                }
            });

            foreach (GameObject o in tempList) transparentList.Remove(o);

            for (i = 0; i < hitArr.Length; i++)
            {
                if (!transparentList.Contains(hitArr[i].transform.gameObject))
                    transparentList.Add(hitArr[i].transform.gameObject);
            }

            for (i = 0; i < transparentList.Count; i++)
            {
                MeshRenderer[] mrArr = transparentList[i].GetComponentsInChildren<MeshRenderer>();
                for (j = 0; j < mrArr.Length; j++)
                {
                    mrArr[j].enabled = false;
                }
            }
        }
    }*/
}

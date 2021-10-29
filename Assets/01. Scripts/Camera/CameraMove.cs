using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraMove : MonoBehaviour
{
    [SerializeField] Vector3 defaultCamMinPos, defaultCamMaxPos;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform rotTarget;
    [HideInInspector] public PlayerScript player;
    public JoystickControl joystick;
    public Vector3 camMinPos, camMaxPos;

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

    private RaycastHit[] hitArr;
    private float trpTime;
    private List<GameObject> transparentList = new List<GameObject>();
    public LayerMask whatIsWall;


    private float ClampAngle(float angle, float min, float max)  //카메라 앵글 제한
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
            transform.position = FunctionGroup.PositionLimit(transform.position, camMinPos, camMaxPos);
            ObjectTransparency();
            //Debug.DrawRay(transform.position, (target.position - transform.position).normalized * Vector3.Distance(transform.position, target.position), Color.blue);
        }
    }


    private void CamMove()  
    {
        position = rotation * Offset + target.position;
        transform.position = position;

        for(int i=0; i<Input.touchCount; i++)  //터치된 모든 손가락 검사
        {
            Touch t = Input.GetTouch(i);

            if (EventSystem.current.IsPointerOverGameObject(t.fingerId) && t.fingerId!=rotFingerId) continue;
             
            switch (t.phase)
            {
                case TouchPhase.Began:  //터치 시작
                    if (rotFingerId == -1)
                    {
                        rotFingerId = t.fingerId;
                        firstPoint = t.position;
                    }
                    break;

                case TouchPhase.Moved:  //터치하고 움직이는 중
                    if (t.fingerId == rotFingerId)
                    {
                        Move(t,true);
                    }
                    break;

                case TouchPhase.Stationary:  //터치해놓고 가만히 있는중
                    if(t.fingerId==rotFingerId)
                    {
                        Move(t,false);
                    }
                    break;

                case TouchPhase.Ended:  //터치 끝
                    if(t.fingerId==rotFingerId)
                    {
                        rotFingerId = -1;
                    }
                    break;

                case TouchPhase.Canceled:  //터치 취소
                    if (t.fingerId == rotFingerId)
                    {
                        rotFingerId = -1;
                    } 
                    break;
            }
        }

        if (isPCModeRotateCam)  //PC로 할 때 카메라 움직임 처리
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

    private void Move(Touch t, bool isFingerMoved=true)  //여기서 실제로 움직이고 회전
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

    private void PlayerRotation()  //플레이어를 회전시킴
    {
        if ((joystick.PC_MoveDir!=Vector3.zero || joystick.isTouch) && !player.isJumping)
        {
            rotTarget.rotation = Quaternion.Slerp(rotTarget.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * player.rotateSpeed);
        }
    }

    public void ResetRange()  //카메라 위치 범위 초기화
    {
        camMinPos = defaultCamMinPos;
        camMaxPos = defaultCamMaxPos;
    }

    public void Setting(Transform target, Transform rotTarget)
    {
        this.target = target;
        this.rotTarget = rotTarget;
    }

    private void ObjectTransparency()  //카메라와 플레이어 사이의 오브젝트의 메쉬를 꺼준다
    {
        if (trpTime < Time.time)
        {
            trpTime = Time.time + 0.1f;

            float dist = Vector3.Distance(transform.position, target.position);
            hitArr = Physics.RaycastAll(transform.position, (target.position-transform.position).normalized, dist, whatIsWall);

            int i, j;

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
                    transparentList.Remove(x);
                }
            });

            for (i = 0; i < hitArr.Length; i++)
            {
                if(!transparentList.Contains(hitArr[i].transform.gameObject))
                   transparentList.Add(hitArr[i].transform.gameObject);
            }

            for(i=0; i<transparentList.Count; i++)
            {
                MeshRenderer[] mrArr = transparentList[i].GetComponentsInChildren<MeshRenderer>();
                for(j=0; j<mrArr.Length; j++)
                {
                    mrArr[j].enabled = false;
                }
            }
        }
    }
}

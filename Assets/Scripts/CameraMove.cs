using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform rotTarget;
    [HideInInspector] public PlayerScript player;
    public JoystickControl joystick;

    public float xSpeed = 220f, ySpeed = 100f;
    [HideInInspector] public float x, y;
    public float yMinLimit = -20f, yMaxLimit = 80f;

    public Vector3 Offset;
    private Vector3 position;
    private Quaternion rotation;

    private int rotFingerId;

    private float screenWidth, screenHeight;

    private Vector3 firstPoint, secondPoint;

    private float ClampAngle(float angle, float min, float max)
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
        }
    }

    private void CamMove()
    {
        position = rotation * Offset + target.position;
        transform.position = position;

        for(int i=0; i<Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (EventSystem.current.IsPointerOverGameObject(t.fingerId) && t.fingerId!=rotFingerId) continue;
             
            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (rotFingerId == -1)
                    {
                        rotFingerId = t.fingerId;
                        firstPoint = t.position;
                    }
                    break;

                case TouchPhase.Moved:
                    if (t.fingerId == rotFingerId)
                    {
                        Move(t);
                    }
                    break;

                case TouchPhase.Stationary:
                    if(t.fingerId==rotFingerId)
                    {
                        Move(t);
                    }
                    break;

                case TouchPhase.Ended:
                    if(t.fingerId==rotFingerId)
                    {
                        rotFingerId = -1;
                    }
                    break;

                case TouchPhase.Canceled:
                    if (t.fingerId == rotFingerId)
                    {
                        rotFingerId = -1;
                    } 
                    break;
            }
        }
    }

    private void Move(Touch t)
    {
        secondPoint = t.position;
        x += (secondPoint.x - firstPoint.x) * 180 / screenWidth;
        y -= (secondPoint.y - firstPoint.y) * 90 / screenHeight;
        y = ClampAngle(y, yMinLimit, yMaxLimit);

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

    private void PlayerRotation()
    {
        if (joystick.isTouch && !player.isJumping)
        {
            rotTarget.rotation = Quaternion.Slerp(rotTarget.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * player.rotateSpeed);
        }
    }
}

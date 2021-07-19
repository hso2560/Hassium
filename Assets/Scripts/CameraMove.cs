using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform rotTarget;
    [HideInInspector] public PlayerScript player;
    public JoystickControl joystick;

    public float xSpeed = 220f, ySpeed = 100f;
    public float x, y;
    public float yMinLimit = -20f, yMaxLimit = 80f;

    public Vector3 Offset;
    private Vector3 position;
    private Quaternion rotation;

    private float halfScreenWidth;
    private int rightFingerId;

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

        halfScreenWidth = Screen.width / 2;
        rightFingerId = -1;
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

            if (EventSystem.current.IsPointerOverGameObject(t.fingerId) && t.fingerId!=rightFingerId) continue;
             
            switch (t.phase)
            {
                case TouchPhase.Began:
                    rightFingerId = t.fingerId;
                    break;

                case TouchPhase.Moved:
                    if (t.fingerId == rightFingerId)
                    {
                        Move();
                    }
                    break;

                case TouchPhase.Stationary:
                    if(t.fingerId==rightFingerId)
                    {
                        Move();
                    }
                    break;

                case TouchPhase.Ended:
                    if(t.fingerId==rightFingerId)
                    {
                        rightFingerId = -1;
                    }
                    break;

                case TouchPhase.Canceled:
                    if (t.fingerId == rightFingerId)
                    {
                        rightFingerId = -1;
                    }
                    break;
            }
        }
    }

    private void Move()
    {
        x += Input.GetAxis("Mouse X") * xSpeed * 0.015f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.015f;
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        rotation = Quaternion.Euler(y, x, 0);
        position = rotation * Offset + target.position;

        transform.position = position;
        transform.rotation = rotation;
    }

    private void PlayerRotation()
    {
        if (joystick.isTouch)
        {
            rotTarget.rotation = Quaternion.Slerp(rotTarget.rotation, Quaternion.Euler(0, x, 0), Time.deltaTime * player.rotateSpeed);
        }
    }
}

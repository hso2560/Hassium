
using UnityEngine;
using UnityEngine.UI;

public class SlotManager : MonoBehaviour  //대충 짠 테스트용 코드
{
    public static SlotManager instance;

    public Image mImg;
    public bool isDragging;

    public Image beginImg;


    private Vector3 firstPos;
    public float dist = 10;
    private float clickTime;

    private void Awake()
    {
        instance = this;
    }

    public void Begin(bool b, Image i = null)
    {
        mImg.gameObject.SetActive(b);
        isDragging = b;

        if (i != null)
        {
            mImg.sprite = i.sprite;
            beginImg = i;
        }
    }

    public void Exchange(Image i)
    {
        Sprite temp = i.sprite;
        i.sprite = mImg.sprite;
        beginImg.sprite = temp;
    }

    public void Change(Image i)
    {
        i.gameObject.SetActive(true);
        i.sprite = mImg.sprite;
        beginImg.gameObject.SetActive(false);
    }


    private void Update()  //드래그로 UI꺼낼 때
    {
       if(Input.GetMouseButtonDown(0))
        {
            firstPos = Input.mousePosition;
            clickTime = Time.time;
        }

       if(Input.GetMouseButtonUp(0))
        {
            /*if(Vector3.Distance(Input.mousePosition,firstPos)>dist)
            {
                Debug.Log("Mouse");
            }*/

            if(Input.mousePosition.x - firstPos.x > dist && Input.mousePosition.x<Screen.width/2)
            {
                if(Time.time-clickTime<0.8f)
                    Debug.Log("Mouse");
            }
        }
    }
}

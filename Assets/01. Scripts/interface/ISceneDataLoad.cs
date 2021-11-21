using UnityEngine;
//매니저 오브젝트들에 붙일 인터페이스
//맨 처음 씬에서 SceneObjects를 통해서 아래의 함수를 사용해 모든 매니저 오브젝트들을 소환시킬거임
//근데 원래는 이런식으로 안해도 되고 안하는게 더 편했을 듯. (그냥 이렇게 한 번 해본건데 괜히 더 불편하더라)
public interface ISceneDataLoad
{
    public void ManagerDataLoad(GameObject sceneObjs); //처음에 준비함

    public bool GetReadyState  //준비된 상태인가
    {
        get;
        set;
    }
}

using UnityEngine;
//매니저 오브젝트들에 붙일 인터페이스
//맨 처음 씬에서 SceneObjects를 통해서 아래의 함수를 사용해 모든 매니저 오브젝트들을 소환시킬거임
public interface ISceneDataLoad
{
    public void ManagerDataLoad(GameObject sceneObjs);
}

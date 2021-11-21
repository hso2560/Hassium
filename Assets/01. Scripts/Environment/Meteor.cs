using UnityEngine;

public class Meteor : MonoBehaviour //쓰긴 하지만 딱히 필요는 없는 스크립트(할거 없을 때 걍 한거)
{
    [SerializeField] private MeteorData data;
    [SerializeField] private ParticleSystem effect;
    private Flare flare;

    private LensFlare lensFlare;
    private float speed;
    private Vector3 target;

    private bool isMove, isDropped;

    private void Awake()
    {
        lensFlare = GetComponent<LensFlare>();
        flare = lensFlare.flare;
    }

    private void OnEnable()
    {
        //transform.position = data.startPoints[Random.Range(0, data.startPoints.Length)];
        //target = data.endPoints[Random.Range(0, data.endPoints.Length)];

        transform.position = FunctionGroup.GetRandomVector(data.startPoints[0], data.startPoints[1]);
        target = FunctionGroup.GetRandomVector(data.endPoints[0], data.endPoints[1]);

        lensFlare.flare = flare;
        speed = Random.Range(data.minSpeed, data.maxSpeed);
        lensFlare.brightness = Random.Range(data.minBrightNess, data.maxBrightNess);
        lensFlare.color = data.colors[Random.Range(0, data.colors.Length)];

        ParticleSystem.MainModule main = effect.main;
        main.startColor = lensFlare.color;
        ParticleSystem.VelocityOverLifetimeModule psvol = effect.velocityOverLifetime;
        psvol.orbitalX = Random.Range(-0.35f, 0.35f);
        psvol.orbitalY = Random.Range(-0.2f, 0.2f);
        
        isMove = true;
    }

    private void Update()
    {
        if(isMove)
        {
            transform.position += (target-transform.position).normalized * speed * Time.deltaTime;

            if (Vector3.SqrMagnitude(target-transform.position) <= 16)
                End();
        }
        if(isDropped)
        {
            lensFlare.brightness -= Time.deltaTime;
            if (lensFlare.brightness <= 0)
            {
                isDropped = false;
                gameObject.SetActive(false);
            }
        }
    }

    private void End()
    {
        isMove = false;

        lensFlare.flare = data.flare;
        lensFlare.brightness = Random.Range(1.3f, 1.5f);

        isDropped = true;
    }
}

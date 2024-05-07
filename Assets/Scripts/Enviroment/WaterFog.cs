using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaterFog : MonoBehaviour
{
    public Gradient fogGradient;
    public float fogAtBottomDistance;
    public float fogAtStartDistance;
    [Range (-1f, -150f)] public float fogTransition = -100f;

    private Transform playerPos;

    private readonly float startingFogEndDistance = 300f;

    private Bloom underwaterBloom;
    public float underwaterBloomBaseValue;
    [Range(-1f, -5f)] public float bloomTransition = -2f;

    public AK.Wwise.Event water;
    public AK.Wwise.Event stopWater;

    void Start()
    {
        Volume underwaterVolume;

        playerPos = GameObject.FindWithTag("Spawner").transform;
        underwaterVolume = gameObject.GetComponent<Volume>();

        if (underwaterVolume.profile.TryGet(out Bloom temp)) { underwaterBloom = temp; }

        underwaterBloomBaseValue = underwaterBloom.threshold.value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stopWater.Post(other.gameObject);

            DayAndNightCycle.Instance.OverrideFogColour = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            water.Post(other.gameObject);
            DayAndNightCycle.Instance.OverrideFogColour = false;
        }
    }

    void Update()
    {
        if (playerPos.position.y >= 0f)
        {
            if (!GameManager.Instance.cutSceneOverride)
                RenderSettings.fogEndDistance = startingFogEndDistance;

            return;
        }

        print("effecting");

        float farDown = Mathf.InverseLerp(0f, fogTransition, playerPos.position.y);

        RenderSettings.fogColor = fogGradient.Evaluate(farDown);

        float fogDepth = Mathf.Lerp(fogAtStartDistance, fogAtBottomDistance, farDown);
        RenderSettings.fogEndDistance = fogDepth;

        float farDown2 = Mathf.InverseLerp(0f, bloomTransition, playerPos.position.y);
        float bloomDepth = Mathf.Lerp(2f, underwaterBloomBaseValue, farDown2);

        underwaterBloom.threshold.Override(bloomDepth);
    }
}

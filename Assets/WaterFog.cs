using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaterFog : MonoBehaviour
{
    public Gradient fogGradient;
    public float fogDepth;

    private Transform playerPos;

    private readonly float startingfogEndDistance = 300f;

    private Volume underwaterVolume;
    private Bloom underwaterBloom;

    void Start()
    {
        playerPos = GameObject.FindWithTag("Spawner").transform;
        underwaterVolume = gameObject.GetComponent<Volume>();

        if (underwaterVolume.profile.TryGet(out Bloom temp)) { underwaterBloom = temp; }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            DayAndNightCycle.Instance.OverrideFogColour = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            DayAndNightCycle.Instance.OverrideFogColour = false;
    }

    void Update()
    {
        if (playerPos.position.y >= 0f && !GameManager.Instance.cutSceneOverride)
        {
            RenderSettings.fogEndDistance = startingfogEndDistance;
            return;
        }

        float farDown = Mathf.InverseLerp(0f, -100f, playerPos.position.y);

        RenderSettings.fogColor = fogGradient.Evaluate(farDown);
        RenderSettings.fogEndDistance = fogDepth;

        float farDown2 = Mathf.InverseLerp(0f, -2f, playerPos.position.y);
        float bloomTransition = Mathf.Lerp(2f, 0.5f, farDown2);

        underwaterBloom.threshold.Override(bloomTransition);
    }
}

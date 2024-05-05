using UnityEngine;

[RequireComponent (typeof(LightIsTimeAffected))]
public class LightIsEffectedUnderwater : MonoBehaviour
{
    private LightIsTimeAffected lightEffect;
    private float baseIntensity;

    public float underwaterIntensity;

    // Start is called before the first frame update
    void Start()
    {
        lightEffect = GetComponent<LightIsTimeAffected>();
        baseIntensity = lightEffect.lightIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 0f)
            lightEffect.lightIntensity = underwaterIntensity;
        else
            lightEffect.lightIntensity = baseIntensity;
    }
}

using UnityEngine;

public class LightIsTimeAffected : MonoBehaviour
{
    public bool Invert;

    private Light objectLight;
    private float lightIntensity;

    private void Start()
    {
        if (TryGetComponent<Light>(out var light))
        {
            objectLight = light;
            lightIntensity = objectLight.intensity;
        }
        else
        {
            if (gameObject.transform.parent == null)
                Debug.LogError($"{gameObject.name} missing light component!!!");
            else
                Debug.LogError($"{gameObject.name} on {gameObject.transform.parent.name} ({gameObject.transform.root.name}) missing light component!!!");
            enabled = false;
        }
    }

    void Update()
    {
        float newlightIntensity;

        if (!Invert)
            newlightIntensity = DayAndNightCycle.Instance.GetTimeToDay(0f, lightIntensity);
        else
            newlightIntensity = DayAndNightCycle.Instance.GetTimeToNight(0f, lightIntensity);

        objectLight.intensity = newlightIntensity;
    }
}

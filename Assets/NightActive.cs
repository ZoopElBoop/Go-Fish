using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightActive : MonoBehaviour
{
    public Light[] objectLight;
    public float lightLimit;

    void Update()
    {
        float lightIntensity = DayAndNightCycle.Instance.GetTimeToDay(0f, lightLimit);

        if (lightIntensity != -1)
        {
            foreach (var light in objectLight)
            {
                light.intensity = lightIntensity;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("Gradients")]
    [SerializeField] private Gradient fogGradient;
    [SerializeField] private Gradient ambientGradient;
    [SerializeField] private Gradient directionalLightGradient;
    [SerializeField] private Gradient skyboxTintGradient;
    [SerializeField] private Gradient waterTintGradient;

    [Header("Enviromental")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Material waterMaterial;

    [Header("Settings")]
    [SerializeField] private float dayTimeDuration;
    [SerializeField] private float rotationSpeed;

    private float currentTime;

    private void Update()
    {
        UpdateTime();
        UpdateDayAndNightCycle();
        RotateSkyBox();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime / dayTimeDuration;
        currentTime = Mathf.Repeat(currentTime, 1);
    }

    private void UpdateDayAndNightCycle() 
    {
        float sunPos = Mathf.Repeat(currentTime + 0.25f, 1f);
        directionalLight.transform.rotation = Quaternion.Euler(sunPos * 360f, 0f, 0f);

        RenderSettings.fogColor = fogGradient.Evaluate(currentTime);
        RenderSettings.ambientLight = ambientGradient.Evaluate(currentTime);

        directionalLight.color = directionalLightGradient.Evaluate(currentTime);

        skyboxMaterial.SetColor("_Tint", skyboxTintGradient.Evaluate(currentTime));
        //waterMaterial.SetFloat("_Glossiness", currentTime);
        //print(waterMaterial.Get("_Glossiness"));
    }

    private void RotateSkyBox() 
    {
        float currentRotation = skyboxMaterial.GetFloat("_Rotation");
        float newRotation = currentRotation + rotationSpeed * Time.deltaTime;

        newRotation = Mathf.Repeat(newRotation, 360f);
        skyboxMaterial.SetFloat("_Rotation", newRotation);
    }

    private void OnApplicationQuit()
    {
        skyboxMaterial.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f));

    }
}

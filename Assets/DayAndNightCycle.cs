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

    [Header("Enviromental")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Material waterMaterial;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed;
    [SerializeField][Range(0, 0.5f)] private float dayStart = 0.2f;
    [SerializeField][Range(0.5f, 1)] private float dayEnd = 0.8f;

    public float aaaaaaaa;
    private void Update()
    {
        UpdateDayAndNightCycle();
        RotateSkyBox();
    }

    private void UpdateDayAndNightCycle() 
    {
        float currentTime = GameManager.Instance.GetGameTime();

        float sunPos = Mathf.Repeat(currentTime + 0.25f, 1f);
        directionalLight.transform.rotation = Quaternion.Euler(sunPos * 360f, 0f, 0f);

        RenderSettings.fogColor = fogGradient.Evaluate(currentTime);
        RenderSettings.ambientLight = ambientGradient.Evaluate(currentTime);

        directionalLight.color = directionalLightGradient.Evaluate(currentTime);

        skyboxMaterial.SetColor("_Tint", skyboxTintGradient.Evaluate(currentTime));
        //waterMaterial.SetFloat("_Glossiness", currentTime);
        //print(waterMaterial.Get("_Glossiness"));

        ChangeSeaGlossiness(currentTime);
    }

    private void ChangeSeaGlossiness(float time) 
    {

        if (time >= dayStart && time < 0.5f)       
            time = Mathf.InverseLerp(0f, dayStart, time);    
        else if (time > 0.5f && time <= dayEnd)        
            time = Mathf.InverseLerp(dayEnd, 1f, time);

        aaaaaaaa = time;

        waterMaterial.SetFloat("_Glossiness", time);
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
        waterMaterial.SetFloat("_Glossiness", 1f);
    }
}

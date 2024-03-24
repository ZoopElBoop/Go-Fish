using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DayAndNightCycle : MonoBehaviour
{
    [Header("In-Game Time (In Seconds)")]
    [SerializeField][Min(1)] private float dayTimeCycle;
    [Range(0.0f, 1.0f)] public float gameTime;
    [SerializeField] private bool overrideGameTimeReset = false;
    public bool _freezeGameTime = false;

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
    [SerializeField][Range(0, 0.25f)] private float minimumWaterGlossiness;
    [SerializeField][Range(0.25f, 1)] private float maximumWaterGlossiness;
    [Space]
    [SerializeField][Range(0, 0.45f)] private float nightStart = 0.2f;
    [SerializeField][Range(0.55f, 1)] private float nightEnd = 0.8f;
    [SerializeField][Range(0f, 0.1f)] private float dayToNightTransition = 0.05f;

    private void Start()
    {
        if (!overrideGameTimeReset)
            gameTime = 0f;
    }

    private void Update()
    {
        UpdateGameTime();
        UpdateDayAndNightCycle();
        RotateSkyBox();
    }

    private void UpdateGameTime() 
    {
        if (!_freezeGameTime)
        {
            gameTime += Time.deltaTime / dayTimeCycle;
            gameTime = Mathf.Repeat(gameTime, 1);
        }
    }

    private void UpdateDayAndNightCycle()
    {
        float currentTime = 0f;

        currentTime = gameTime;

        float sunPos = Mathf.Repeat(currentTime + 0.25f, 1f);
        directionalLight.transform.rotation = Quaternion.Euler(sunPos * 360f, 0f, 0f);

        RenderSettings.fogColor = fogGradient.Evaluate(currentTime);
        RenderSettings.ambientLight = ambientGradient.Evaluate(currentTime);

        directionalLight.color = directionalLightGradient.Evaluate(currentTime);

        skyboxMaterial.SetColor("_Tint", skyboxTintGradient.Evaluate(currentTime));

        ChangeSeaGlossiness(currentTime);
    }

    private void ChangeSeaGlossiness(float time) 
    {
        if (time < nightStart || time > nightEnd)
            return;
        else if (time > nightStart + dayToNightTransition && time < nightEnd - dayToNightTransition)
            return;

        if (time >= nightStart && time < nightStart + dayToNightTransition)
        {
            time = Mathf.InverseLerp(nightStart + dayToNightTransition, nightStart, time);
            time = Mathf.Lerp(minimumWaterGlossiness, maximumWaterGlossiness, time);
        }
        else if (time >= nightEnd - dayToNightTransition)
        {
            time = Mathf.InverseLerp(nightEnd - dayToNightTransition, nightEnd, time);
            time = Mathf.Lerp(minimumWaterGlossiness, maximumWaterGlossiness, time);
        }

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
        waterMaterial.SetFloat("_Glossiness", maximumWaterGlossiness);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DayAndNightCycle : MonoBehaviour
{
    public static DayAndNightCycle Instance;

    [Header("In-Game Time (In Seconds)")]
    [SerializeField][Min(1)] private float dayTimeCycle = 60f; 
    [SerializeField] private bool overrideGameTimeReset = false;
    [SerializeField] private bool freezeGameTime = false;
    [Space]
    [SerializeField][Range(0.0f, 1.0f)] private float gameTime;
    [SerializeField][Range(0.0f, 1.0f)] private float transitionTime;
    [SerializeField][Min(0)] private int numOfDays = 0;

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
    [SerializeField][Range(0, 0.3f)] private float nightStart = 0.2f;
    [SerializeField][Range(0.7f, 1)] private float nightEnd = 0.8f;
    [SerializeField][Range(0f, 0.2f)] private float dayToNightTransition = 0.1f;

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        Instance = this;

        if (!overrideGameTimeReset)
        {
            numOfDays = 0;
            gameTime = 0f;
        }
    }
    
    private void Update()
    {
        UpdateGameTime();
        transitionTime = CalculateTransitionTime(gameTime);

        UpdateDayAndNightCycle();
        RotateSkyBox();
    }

    private void UpdateGameTime() 
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (!freezeGameTime)
        {
            gameTime += Time.deltaTime / dayTimeCycle;

            if (gameTime >= 1f)
            {
                numOfDays++;
                gameTime = 0f;
            }
        }
    }

    private void UpdateDayAndNightCycle()
    {
        float sunPos = Mathf.Repeat(gameTime + 0.25f, 1f);
        directionalLight.transform.rotation = Quaternion.Euler(sunPos * 360f, 0f, 0f);

        RenderSettings.fogColor = fogGradient.Evaluate(gameTime);
        RenderSettings.ambientLight = ambientGradient.Evaluate(gameTime);

        directionalLight.color = directionalLightGradient.Evaluate(gameTime);

        skyboxMaterial.SetColor("_Tint", skyboxTintGradient.Evaluate(gameTime));

        ChangeSeaGlossiness();
    }

    private void ChangeSeaGlossiness() 
    {
        float glossiness = GetTimeToNight(minimumWaterGlossiness, maximumWaterGlossiness);

        waterMaterial.SetFloat("_Glossiness", glossiness);
    }

    private void RotateSkyBox() 
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        float currentRotation = skyboxMaterial.GetFloat("_Rotation");
        float newRotation = currentRotation + rotationSpeed * Time.deltaTime;

        newRotation = Mathf.Repeat(newRotation, 360f);
        skyboxMaterial.SetFloat("_Rotation", newRotation);
    }

    #region Cycle Transition Time Functions

    //Calculates how far through transition to night from day
    // 1 = day
    // 0 = night
    private float CalculateTransitionTime(float time)
    {
        if (time < nightStart || time > nightEnd)   //returns as still in day
            return 1f;
        else if (time > nightStart + dayToNightTransition && time < nightEnd - dayToNightTransition)    //returns as in night
            return 0f;

        //gets value between 0-1 of how far it is into day/night transition
        if (time <= nightStart + dayToNightTransition)  //transitioning to night
            time = Mathf.InverseLerp(nightStart + dayToNightTransition, nightStart, time);
        else                                            //transitioning to day
            time = Mathf.InverseLerp(nightEnd - dayToNightTransition, nightEnd, time);

        return time;
    }

    //Lerps betweem min & max depending on how far through transition
    //max = day
    //min = night
    public float GetTimeToNight(float min, float max)
    {
        return Mathf.Lerp(min, max, transitionTime);
    }

    //Lerps betweem min & max depending on how far through transition & inverts value
    //min = day
    //max = night
    public float GetTimeToDay(float min, float max)
    {
        return max - GetTimeToNight(min, max);
    }

    #endregion

    public float GetTime() { return gameTime; }
    public int GetDay() { return numOfDays; }
    public void FreezeTime(bool status) { freezeGameTime = status; }

    private void OnApplicationQuit()
    {
        skyboxMaterial.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f));
        skyboxMaterial.SetFloat("_Rotation", 0f);
        waterMaterial.SetFloat("_Glossiness", maximumWaterGlossiness);
    }
}

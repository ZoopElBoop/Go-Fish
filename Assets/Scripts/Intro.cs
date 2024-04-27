using System;
using UnityEngine;

public class Intro : MonoBehaviour
{
    #region Enviromental Objects

    [Header("Enviromental Objects")]

    public DayAndNightCycle timeCycle;

    public Light Light;
    private float startingIntensity;

    public float fogDistance;
    private float fogDistanceNormal;

    public Gradient ambientToNormal;

    private Material skyboxMat;
    private float baseExposure;

    #endregion

    [Header("Time Setting")]

    public float transitionTimeInSeconds = 1f;
    private float transitionTime = 0f;

    private bool activated = false;
    private bool lightsOn = false;

    [Header("Intro Objects")]

    public GameObject bounds;
    public GameObject introLetter;


    private void Awake()
    {
        EnviromentalValuesInit();

        introLetter.SetActive(false);
        bounds.SetActive(true);

        enabled = false;
    }

    private void OnEnable()
    {
        if (!activated)
            activated = true;

        ShowLetter();
    }

    private void EnviromentalValuesInit()
    {
        startingIntensity = Light.intensity;
        Light.intensity = 0f;

        fogDistanceNormal = RenderSettings.fogEndDistance;
        RenderSettings.fogEndDistance = fogDistance;

        RenderSettings.ambientLight = new Color(0f, 0f, 0f);

        skyboxMat = RenderSettings.skybox;
        baseExposure = skyboxMat.GetFloat("_Exposure");
        skyboxMat.SetFloat("_Exposure", 0f);

    }

    private void Update()
    {
        if (Input.anyKeyDown)
            HideLetter();

        if (!lightsOn && !introLetter.activeSelf)
        {
            transitionTime += Time.deltaTime / transitionTimeInSeconds;

            EnableLighting();
            RemoveBounds();

            if (transitionTime >= 1f)
            {
                timeCycle.enabled = true;

                lightsOn = true;
                enabled = false;
            }
        }
    }

    private void ShowLetter() 
    {
        introLetter.SetActive(true);

        PlayerScriptManager.Instance.ShutDown("Interact", false);
        PlayerScriptManager.Instance.ShutDown("Movement", false);

        GameManager.Instance.ShowPlayerMouse(false);
    }

    private void HideLetter() 
    {
        introLetter.SetActive(false);

        PlayerScriptManager.Instance.ShutDown("Interact", true);
        PlayerScriptManager.Instance.ShutDown("Movement", true);

        if (lightsOn)
            enabled = false;
    }

    private void EnableLighting() 
    {
        Light.intensity = Mathf.Lerp(0f, startingIntensity, transitionTime);

        skyboxMat.SetFloat("_Exposure", Mathf.Lerp(0f, baseExposure, transitionTime));

        RenderSettings.fogEndDistance = Mathf.Lerp(fogDistance, fogDistanceNormal, transitionTime);

        RenderSettings.ambientLight = ambientToNormal.Evaluate(transitionTime);
    }

    private void RemoveBounds() 
    {
        bounds.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        skyboxMat.SetFloat("_Exposure", baseExposure);
    }
}

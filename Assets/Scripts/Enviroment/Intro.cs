using UnityEngine;

public class Intro : MonoBehaviour
{
    #region Enviromental Objects

    [Header("Enviromental Objects")]

    public DayAndNightCycle timeCycle;

    public Light sceneLight;
    private float sceneStartingIntensity;

    public Light letterLight;

    public float fogDistance;
    private float fogDistanceNormal;

    public Gradient ambientToNormal;

    private Material skyboxMat;
    private float baseExposure;
    private float baseFogFill;

    private MoveLock bounds;

    #endregion

    [Header("Time Setting")]

    public float titleTextTimeInSeconds = 1f;
    public float transitionTimeInSeconds = 1f;
    private float transitionTime = 0f;

    private bool activated = false;
    private bool lightsOn = false;
    private bool letterActive = false;

    private void Awake()
    {
        EnviromentalValuesInit();

        bounds = GetComponent<MoveLock>();

        enabled = false;
    }

    private void OnEnable()
    {
        ShowLetter();
    }

    private void EnviromentalValuesInit()
    {
        sceneStartingIntensity = sceneLight.intensity;
        sceneLight.intensity = 0f;

        fogDistanceNormal = RenderSettings.fogEndDistance;
        RenderSettings.fogEndDistance = fogDistance;

        RenderSettings.ambientLight = new Color(0f, 0f, 0f);

        skyboxMat = RenderSettings.skybox;

        baseExposure = skyboxMat.GetFloat("_Exposure");
        skyboxMat.SetFloat("_Exposure", 0f);

        baseFogFill = skyboxMat.GetFloat("_FogFill");
        skyboxMat.SetFloat("_FogFill", 1f);
    }

    private void Update()
    {
        if (Input.anyKeyDown && letterActive)
            HideLetter();

        if (!lightsOn && !letterActive)
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
        print("aaaa");
        letterActive = true;
        UIManager.Instance.LetterStatus(letterActive);

        PlayerScriptManager.Instance.ShutDown("Interact", false);
        PlayerScriptManager.Instance.ShutDown("Movement", false); //reenabled in UIMANAGER

        GameManager.Instance.ShowPlayerMouse(false);
    }

    private void HideLetter() 
    {
        letterActive = false;
        UIManager.Instance.LetterStatus(letterActive);

        PlayerScriptManager.Instance.ShutDown("Movement", true);


        if (!activated) 
        {
            activated = true;
            UIManager.Instance.StartIntroText(transitionTimeInSeconds, titleTextTimeInSeconds);
        }else
            PlayerScriptManager.Instance.ShutDown("Interact", true);

        if (lightsOn)
            enabled = false;
    }

    private void EnableLighting() 
    {
        sceneLight.intensity = Mathf.Lerp(0f, sceneStartingIntensity, transitionTime);
        letterLight.intensity = Mathf.Lerp(letterLight.intensity, 0f, transitionTime);

        skyboxMat.SetFloat("_Exposure", Mathf.Lerp(0f, baseExposure, transitionTime));
        skyboxMat.SetFloat("_FogFill", Mathf.Lerp(1f, baseFogFill, transitionTime));

        RenderSettings.fogEndDistance = Mathf.Lerp(fogDistance, fogDistanceNormal, transitionTime);

        RenderSettings.ambientLight = ambientToNormal.Evaluate(transitionTime);
    }

    private void RemoveBounds() 
    {
        bounds.enabled = false;
    }

    private void OnApplicationQuit()
    {
        skyboxMat.SetFloat("_Exposure", baseExposure);
        skyboxMat.SetFloat("_FogFill", baseFogFill);
    }
}

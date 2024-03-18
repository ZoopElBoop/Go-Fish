using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Fishing Slider")]
    [SerializeField] private Slider fishCaughtSlider;
    [SerializeField] private float barSpeed = 0.25f;
    private bool barAtTop;

    [Header("Throw Slider")]
    [SerializeField] private Slider fishingThrowSlider;

    [Header("Fish Caught")]
    [SerializeField] private TMP_Text catches;

    private void Awake()
    {
        if (Instance == null)        
            Instance = this;       
        else
            Destroy(this);
    }

    private void Update()
    {
        if (fishCaughtSlider.gameObject.activeSelf)
            FishingSlider();
    }

#region FishingSliderFuncs

    private void FishingSlider() 
    {
        if (!barAtTop)
        {
            fishCaughtSlider.value += barSpeed * Time.deltaTime;

            if (fishCaughtSlider.value >= 1)
                barAtTop = true;
        }
        else
        {
            fishCaughtSlider.value -= barSpeed * Time.deltaTime;

            if (fishCaughtSlider.value <= 0)
                barAtTop = false;
        }
    }

    public void FishingSliderActive(bool active)
    {
        fishCaughtSlider.value = 0.5f;
        fishCaughtSlider.gameObject.SetActive(active);
    }

    public float GetFishingSliderValue() { return fishCaughtSlider.value; }

    #endregion

#region ThrowSliderFuncs

    public void ThrowSlider(float maxRange, float throwCharge)
    {
        fishingThrowSlider.value = Mathf.InverseLerp(0f, maxRange, throwCharge);
    }

    public void ThrowSliderActive(bool active)
    {
        fishingThrowSlider.value = 0f;
        fishingThrowSlider.gameObject.SetActive(active);
    }

    #endregion
}

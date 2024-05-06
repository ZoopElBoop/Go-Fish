using System.Collections;
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

    [Header("Fish Active")]
    [SerializeField] private TMP_Text active;

    [Header("Intro UI")]
    [SerializeField] private Image letter;
    [SerializeField] private TMP_Text presentsText;
    [SerializeField] private TMP_Text titleText;


    private void Awake()
    {
        if (Instance == null)        
            Instance = this;       
        else
            Destroy(this);

        letter.enabled = false;
        presentsText.enabled = false;
        titleText.enabled = false;
    }

    private void Update()
    {
        if (fishCaughtSlider.gameObject.activeSelf)
            FishingSlider();

        active.text = $"{GameManager.Instance.GetFishBufferSize()} Fishes";

        catches.text = $"CAUGHT FISH: {InventoryManager.Instance.TotalStored()}";
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
        fishCaughtSlider.value = Random.value;
        fishCaughtSlider.gameObject.SetActive(active);
    }

    public float GetFishingSliderValue() { return fishCaughtSlider.value; }

    #endregion

    #region Throw Slider Funcs

    public void ThrowSlider(float maxRange, float throwCharge)
    {
        fishingThrowSlider.value = Mathf.InverseLerp(0f, maxRange, throwCharge);
    }

    public void ThrowSliderActive(bool active)
    {
        if (fishingThrowSlider != null)
        {
            fishingThrowSlider.value = 0f;

            fishingThrowSlider.gameObject.SetActive(active);
        }
    }

    #endregion


    #region Intro UI

    public void LetterStatus(bool status)
    {
        letter.enabled = status;
    }

    public void StartIntroText(float transitionTime, float titleTime) 
    {
        StartCoroutine(UITransition(transitionTime, titleTime));
    }

    IEnumerator UITransition(float transitionTime, float titleTime)
    {
        presentsText.enabled = true;
        yield return new WaitForSeconds(transitionTime);
        presentsText.enabled = false;

        titleText.enabled = true;
        yield return new WaitForSeconds(titleTime);
        titleText.enabled = false;

        //i dont like this since its being shut off in the intro script but hey it worky
        //why i made the intro script the object that gets enabled/disabled idk, very stupid idea
        PlayerScriptManager.Instance.ShutDown("Interact", true);

    }

    #endregion
}

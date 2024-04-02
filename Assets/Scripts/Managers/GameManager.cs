using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Private Variables (Only for GameManager)

    public static GameManager Instance;

    private List<GameObject> activeFishBuffer = new();          //cannot be seen in inspector as race condition occurs between adding & removing items due to uning inspector overhead 

    //public DayAndNightCycle timeCycle;

    [Header("FPS Cap")]                                                          
    [SerializeField] [Min(1)] private int fpsLimit;

    #endregion

    #region Public Variables (For all scripts)

    [Header("Game Variables")]

    public float fishCoin;
    public Camera mainCam;

    #endregion

    #region Start Function

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);


        Application.targetFrameRate = fpsLimit;

        mainCam = Camera.main;

        //timeCycle = GetComponent<DayAndNightCycle>();
    }

    #endregion

    #region Fish Buffer

    public void AddFishToBuffer(GameObject fishToAdd) 
    {
        activeFishBuffer.Add(fishToAdd);
    }

    public void RemoveFishFromBuffer(GameObject fishToRemove) 
    {
        fishToRemove.SetActive(false);

        activeFishBuffer.RemoveAll(s => s.activeSelf == false);

        ObjectPoolManager.Instance.DespawnObject(fishToRemove);
    }

    public List<GameObject> GetFishBuffer() { return activeFishBuffer; }

    public int GetFishBufferSize() { return activeFishBuffer.Count; }

    #endregion

    public FishControl GetFishConrolScript(GameObject fish)
    {
        if (fish.transform.root.TryGetComponent<FishControl>(out var fc))
            return fc;
        else
        {
            Debug.LogError($"{fish.transform.root.name} missing fish script!!!");
            Debug.Break();
        }      
        return null;
    }


    public void SwitchMainCamera(Camera newMain) { mainCam = newMain; }

    public Camera GetMainCamera() { return mainCam; }
}

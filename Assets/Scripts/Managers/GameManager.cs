using System;
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

    private void AddFishToBuffer(GameObject fishToAdd) 
    {
        activeFishBuffer.Add(fishToAdd);
    }

    private void RemoveFishFromBuffer(GameObject fishToRemove) 
    {
        fishToRemove.SetActive(false);

        activeFishBuffer.RemoveAll(s => s.activeSelf == false);

        ObjectPoolManager.Instance.DespawnObject("Fish", fishToRemove);
    }

    public List<GameObject> GetFishBuffer() { return activeFishBuffer; }
    public int GetFishBufferSize() => activeFishBuffer.Count;

    #endregion

    public FishControl GetFishControlScript(GameObject fish)
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


    #region Spawn & Despawn Functions

    public FishControl SpawnFishAndGetScript(GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        GameObject fish = ObjectPoolManager.Instance.SpawnObject("Fish", toSpawn, position, rotation); 
        AddFishToBuffer(fish);
        return GetFishControlScript(fish);
    }
/*
    public GameObject SpawnFish(GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        return ObjectPoolManager.Instance.SpawnObject("Fish", toSpawn, position, rotation);
    }*/

    public GameObject SpawnHarpoon(GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        return ObjectPoolManager.Instance.SpawnObject("Harpoon", toSpawn, position, rotation);
    }

    public void DestroyFish(GameObject fishToKill)
    {
        ObjectPoolManager.Instance.DespawnObject("Fish", fishToKill);
        RemoveFishFromBuffer(fishToKill);
    }

    public void DestroyHarpoon(GameObject harpoonToKill)
    {
        ObjectPoolManager.Instance.DespawnObject("Harpoon", harpoonToKill);
    }

    #endregion
}

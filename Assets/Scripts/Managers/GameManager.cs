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

        SetPlayerMouse(false);

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
        activeFishBuffer.RemoveAll(s => s.activeSelf == false);
    }

    public List<GameObject> GetFishBuffer() { return activeFishBuffer; }
    public int GetFishBufferSize() { return activeFishBuffer.Count; }

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

    public void SetPlayerMouse(bool status) 
    {
        Cursor.visible = true;

        if (!status)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void SwitchMainCamera(Camera newMain) { mainCam = newMain; }
    public Camera GetMainCamera() { return mainCam; }

    #region Spawn & Despawn Functions

    public dynamic SpawnFish(GameObject toSpawn, Vector3 position, Quaternion rotation, bool returnScript)
    {
        GameObject fish = ObjectPoolManager.Instance.SpawnObject("Fish", toSpawn, position, rotation);
        AddFishToBuffer(fish);

        if (returnScript)
            return GetFishControlScript(fish);
        else
            return fish;
    }

    public void DestroyFish(GameObject fishToKill)
    {
        ObjectPoolManager.Instance.DespawnObject("Fish", fishToKill);
        RemoveFishFromBuffer(fishToKill);
    }

    public GameObject SpawnHarpoon(GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        return ObjectPoolManager.Instance.SpawnObject("Harpoon", toSpawn, position, rotation);
    }
    
    public void DestroyHarpoon(GameObject harpoonToKill)
    {
        ObjectPoolManager.Instance.DespawnObject("Harpoon", harpoonToKill);
    }

    #endregion
}

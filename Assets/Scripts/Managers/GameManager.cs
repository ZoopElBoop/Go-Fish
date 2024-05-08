using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Private Variables (Only for GameManager)

    public static GameManager Instance;

    private List<GameObject> activeFishBuffer = new();          //cannot be seen in inspector as race condition occurs between adding & removing items due to uning inspector overhead 

    [Header("FPS Cap")]                                                          
    [SerializeField] [Min(1)] private int fpsLimit;

    #endregion

    #region Public Variables (For all scripts)

    [Header("Game Variables")]

    //lmao
    public float harpoonSpeed = 15f;
    public int harpoonDamage = 1;

    public int fishCoin;
    public bool InVessel { get; private set; } 

    public bool cutSceneOverride { get; private set; }

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

        ShowPlayerMouse(false);
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

    #region Fish Control Script & Player Mouse Control

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

    public void ShowPlayerMouse(bool status) 
    {
        Cursor.visible = status;

        if (!status)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    #endregion

    #region Vessel Status

    public void CanFish(bool status) => EventManager.Instance.SetFishingStatus(status);

    public void SetVesselStatus(bool status) => InVessel = status;

    #endregion

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

    public void InScene(bool status) => cutSceneOverride = status;
}

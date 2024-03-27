using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<GameObject> activeFishBuffer = new();          //cannot be seen in inspector as race condition occurs between adding & removing items due to uning inspector overhead 
                                                                //basically no look at list in engine :(
    public float fishCoin;

    [Header("FPS Cap")]                                                          
    [SerializeField] [Min(1)] private int fpsLimit;

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
    }

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
}

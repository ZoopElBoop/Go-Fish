using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float fishCoin;

    private List<GameObject> activeFishBuffer = new();          //cannot be seen in inspector as race condition occurs between adding & removing items due to uning inspector overhead 
                                                                //basically no look at list in engine :(
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);  
    }  

    public void AddFishToBuffer(GameObject fishToAdd) 
    {
        activeFishBuffer.Add(fishToAdd); 
    }

    public void RemoveFishFromBuffer(GameObject fishToRemove) 
    {
        activeFishBuffer.Remove(fishToRemove);

        Destroy(fishToRemove); 
    }

    public List<GameObject> GetFishBuffer() { return activeFishBuffer; }

    public int GetFishBufferSize() { return activeFishBuffer.Count; }
}

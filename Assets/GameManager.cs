using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Update is called once per frame
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

    private void Start()
    {
        var fishStoredArr = new List<FishStored>();
    }
}

class FishStored
{
    FishData FishData;
}

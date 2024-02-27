using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishDataManager : MonoBehaviour
{
    public static FishDataManager Instance { get; private set; }

    [Header("DO NOT TOUCH WHILE GAME IS RUNNING pls")]
    public FishData[] fishData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(gameObject);
    }
    private void OnValidate()
    {
/*        //checks to ensure no duplicates found
        for (int i = 0; i < fishData.Length - 1; i++)
        {
            //print(fishData[i]);

            if (fishData[i] == fishData[i + 1])
            {
                Debug.LogError($"Duplicate {fishData[i].name} found, pls fix");
            }
        }*/
        
    }
}
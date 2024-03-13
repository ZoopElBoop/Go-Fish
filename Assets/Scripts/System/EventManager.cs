using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

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

    //When fish is caught by the wobber
    public event Action<GameObject> OnFishFished; 
    public void FishFished(GameObject Catch) 
    {
        OnFishFished?.Invoke(Catch);
    }

    //When player wins the fishing mini-game
    public event Action<GameObject> OnFishCaught;
    public void FishCaught(GameObject caughtFish)
    {
        caughtFish.GetComponent<FishControl>().FishToPlayer();

        OnFishCaught?.Invoke(caughtFish);
    }


    //When fish despwans
    public event Action<GameObject> OnFishDespawn;
    public void FishDespawn(GameObject fishToKill)
    {
        OnFishDespawn?.Invoke(fishToKill);

        GameManager.Instance.RemoveFishFromBuffer(fishToKill);
    }


    //When fish is destroyed
    public event Action<GameObject> OnFishDisable;
    public void FishDisable(GameObject fishToKill)
    {
        OnFishDisable?.Invoke(fishToKill);

        GameManager.Instance.RemoveFishFromBuffer(fishToKill);
    }


    //When player enters boat
    public event Action<Camera> OnBoatEnter;
    public void BoatEnter(Camera boatCam)
    {
        OnBoatEnter?.Invoke(boatCam);
    }

    //when player exits boat
    public event Action OnBoatExit;
    public void BoatExit()
    {
        OnBoatExit?.Invoke();
    }
}

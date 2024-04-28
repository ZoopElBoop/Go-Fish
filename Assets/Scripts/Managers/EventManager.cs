using System;
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

    #region Fishing

    //When fish is caught by the wobber
    // Wob --> Fishing 
    public event Action<GameObject> OnFishCaughtByBobber;
    public void FishCaught(GameObject Catch)
    {
        OnFishCaughtByBobber?.Invoke(Catch);
    }


    //When wobber is too far from player
    // Wobb --> Fishing
    public event Action OnDestroyWobber;
    public void DestroyWobber()
    {
        OnDestroyWobber?.Invoke();
    }


    //When a fish is caught
    //Fish Control --> Fish Shop
    public event Action OnFishCaught;
    public void FishCaught()
    {
        OnFishCaught?.Invoke();
    }


    //If the player can fish
    //Gamemanager --> Fishing
    public event Action<bool>CanFish;
    public void SetFishingStatus(bool status)
    {
        CanFish?.Invoke(status);
    }

    #endregion

    #region Boat Camera Switch

    //When player enters boat
    // Boat --> Fishing
    public event Action<Camera> OnBoatEnter;
    public void BoatEnter(Camera boatCam)
    {
        OnBoatEnter?.Invoke(boatCam);
    }


    //when player exits boat
    // Boat --> Fishing
    public event Action OnBoatExit;
    public void BoatExit()
    {
        OnBoatExit?.Invoke();
    }

    #endregion
}

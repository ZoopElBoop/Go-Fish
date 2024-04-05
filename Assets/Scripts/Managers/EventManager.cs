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
    public event Action<GameObject> OnFishCaught;
    public void FishCaught(GameObject Catch)
    {
        OnFishCaught?.Invoke(Catch);
    }


    //When wobber is too far from player
    // Wobb --> Fishing
    public event Action OnDestroyWobber;
    public void DestroyWobber()
    {
        OnDestroyWobber?.Invoke();
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

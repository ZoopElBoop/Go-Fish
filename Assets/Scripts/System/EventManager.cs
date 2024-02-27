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
            Destroy(gameObject);      
    }

    public event Action<GameObject> OnFishFished; 
    public void FishFished(GameObject Catch) 
    {
        OnFishFished?.Invoke(Catch);
    }


    public event Action<GameObject> OnFishCaught;
    public void FishCaught(GameObject fishToKill)
    {
        OnFishCaught?.Invoke(fishToKill);
    }


    public event Action<Camera> OnBoatEnter;
    public void BoatEnter(Camera boatCam)
    {
        OnBoatEnter?.Invoke(boatCam);
    }


    public event Action OnBoatExit;
    public void BoatExit()
    {
        OnBoatExit?.Invoke();
    }
}

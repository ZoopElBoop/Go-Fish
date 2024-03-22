using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float fishCoin;

    [Header("In-Game Time (In Seconds)")]
    [SerializeField] [Min(1)] private float dayTimeCycle;
    [SerializeField] private float gameTime;

    private List<GameObject> activeFishBuffer = new();          //cannot be seen in inspector as race condition occurs between adding & removing items due to uning inspector overhead 

    [Header("FPS Cap")]                                                          //basically no look at list in engine :(
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

        gameTime = 0f;
    }

    void Update() 
    {
        gameTime += Time.deltaTime / dayTimeCycle;
        gameTime = Mathf.Repeat(gameTime, 1);
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

    public float GetGameTime() { return gameTime; }
}
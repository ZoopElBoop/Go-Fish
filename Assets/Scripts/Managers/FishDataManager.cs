using UnityEngine;

public class FishDataManager : MonoBehaviour
{
    public static FishDataManager Instance { get; private set; }

    [Header("WILL NOT UPDATE WHILE IN PLAY!!!")]
    [SerializeField] public FishData[] fishDataInput;
    
    private FishData[] fishData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);

        fishData = fishDataInput;
    }
/*
    public int GetFishDataIndex(string fishName)
    {
        for (int i = 0; i < fishData.Length; i++)
            if (fishName == fishData[i].name)
                return i;
        return -1;
    }*/

    public int GetFishDataSize() { return fishData.Length; }

    public string GetFishName(int index) { return fishData[index].name; }
    public GameObject GetFish(int index) { return fishData[index]._Fishk; }


    public int GetHealth(int index) { return fishData[index]._Health; }
    public float GetSpeed(int index) { return fishData[index]._Speed; }
    public float GetRotationSpeed(int index) { return fishData[index]._rotationSpeed; }

    public bool GetCanBeCaught(int index) { return fishData[index]._canBeCaught; }
    public float GetSpawnStart(int index) { return fishData[index]._spawnDepthStart; }
    public float GetSpawnHigh(int index) { return fishData[index]._spawnDepthHigh; }
    public float GetSpawnEnd(int index) { return fishData[index]._spawnDepthEnd; }

    public float GetHeightLimit(int index) { return fishData[index]._moveHeightLimit; }
    public float GetDepthLimit(int index) { return fishData[index]._moveDepthLimit;}
    public float GetCollisionRange(int index) { return fishData[index]._collisionRange; }
    public float GetDespawnMultiplier(int index) { return fishData[index]._despawnRangeMulti; }

    public float GetRarity(int index) { return fishData[index]._Rarity; }
    public float GetValue(int index) { return fishData[index]._baseValue; }
    public float GetValueCoefficient(int index) { return fishData[index]._valueChangeCoefficient; }

    public Sprite GetFishImage(int index) { return fishData[index]._FishkImage; }
    public string GetFishDescription(int index) { return fishData[index]._FishkDescription; }
}
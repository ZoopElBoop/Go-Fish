using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<FishStoredData> fishStoredOnPlayer = new();
    [SerializeField] private List<FishStoredData> fishStoredOnBoat = new();
    [SerializeField] private List<FishStoredData> fishStoredOnSub = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);

        Init();
    }

    private void Init()
    {
        fishStoredOnPlayer.Clear();
        fishStoredOnBoat.Clear();
        fishStoredOnSub.Clear();

        for (int i = 0; i < FishDataManager.Instance.GetFishDataSize(); i++)
        {
            fishStoredOnPlayer.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });

            fishStoredOnBoat.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });

            fishStoredOnSub.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });
        }
    }

    public void StoreOnPlayer(FishControl fishScript) { fishStoredOnPlayer[fishScript._dataIndex].count++; }
    public void StoreOnBoat(FishControl fishScript) { fishStoredOnBoat[fishScript._dataIndex].count++; }
    public void StoreOnSub(FishControl fishScript) { fishStoredOnSub[fishScript._dataIndex].count++; }

    public void RemoveFromPlayer(int index) { fishStoredOnPlayer[index].count--; }
    public void RemoveFromBoat(int index) { fishStoredOnBoat[index].count--; }
    public void RemoveFromSub(int index) { fishStoredOnSub[index].count--; }

    public List<FishStoredData> GetFromPlayer() { return fishStoredOnPlayer; }
    public List<FishStoredData> GetFromBoat() { return fishStoredOnBoat; }
    public List<FishStoredData> GetFromSub() { return fishStoredOnSub; }

}

[System.Serializable]
public class FishStoredData
{
    public string fishName;
    public int count;
}

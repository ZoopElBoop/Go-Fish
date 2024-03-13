using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<FishStoredData> fishStoredPlayer = new();
    [SerializeField] private List<FishStoredData> fishStoredBoat = new();
    [SerializeField] private List<FishStoredData> fishStoredSub = new();

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
        for (int i = 0; i < FishDataManager.Instance.GetFishDataSize(); i++)
        {
            fishStoredPlayer.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });

            fishStoredBoat.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });

            fishStoredSub.Add(new FishStoredData
            {
                fishName = FishDataManager.Instance.GetFishName(i),
                count = 0
            });
        }
    }

    public void StoreOnPlayer(FishControl fishScript) { fishStoredPlayer[fishScript._dataIndex].count++; }
    public void StoreOnBoat(FishControl fishScript) { fishStoredBoat[fishScript._dataIndex].count++; }
    public void StoreOnSub(FishControl fishScript) { fishStoredSub[fishScript._dataIndex].count++; }

    public void RemoveFromPlayer(int index) { fishStoredPlayer[index].count--; }
    public void RemoveFromBoat(int index) { fishStoredBoat[index].count--; }
    public void RemoveFromSub(int index) { fishStoredSub[index].count--; }

    public List<FishStoredData> GetFromPlayer() { return fishStoredPlayer; }
    public List<FishStoredData> GetFromBoat() { return fishStoredBoat; }
    public List<FishStoredData> GetFromSub() { return fishStoredSub; }

}

public class FishStoredData
{
    public string fishName;
    public int count;
}

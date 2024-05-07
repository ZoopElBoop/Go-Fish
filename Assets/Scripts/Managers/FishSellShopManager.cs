using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishSellShopManager : MonoBehaviour
{
    public List<FishStoredData> fishStocks = new();
    public SelectionInterface[] shopUiObject;

    public TMP_Text fishToSellText;
    public TMP_Text fishToSellPrice;

    public TMP_Text fishCoinText;

    public GameObject confirmGameObject;

    private int selectedID = 0;

    public AK.Wwise.Event sold; 

    void Start()
    {
        FishShopTextInit();

        UpdateFishShopText();

        EventManager.Instance.OnFishCaught += UpdateFishShopText;
    }

    private void FishShopTextInit()
    {
        for (int i = 0; i < shopUiObject.Length; i++)
        {
            if (shopUiObject[i].Count == null)
                Debug.LogError($"{shopUiObject[i].name} missing fish count text!!!");

            if (shopUiObject[i].Image == null)
                Debug.LogError($"{shopUiObject[i].name} missing fish image!!!");

            shopUiObject[i].ID = i;
        }

        if (shopUiObject.Length < FishDataManager.Instance.GetFishDataSize())
            Debug.LogWarning("Not Enough Ui elements for all fish!!!");

        confirmGameObject.SetActive(false);
    }
    private void UpdateFishShopText()
    {
        print("RESET");

        fishStocks = InventoryManager.Instance.TotalStoredByType();

        for (int i = 0; i < FishDataManager.Instance.GetFishDataSize(); i++)
        {
            shopUiObject[i].Count.text = $"{fishStocks[i].count}";
            shopUiObject[i].Image.sprite = FishDataManager.Instance.GetFishImage(i);
        }

        fishCoinText.text = $"{GameManager.Instance.fishCoin}";

        DisableInteractIfNoFishStock();
    }
    private void DisableInteractIfNoFishStock()
    {
        for (int i = 0; i < FishDataManager.Instance.GetFishDataSize(); i++)
        {
            if (fishStocks[i].count < 1)
                shopUiObject[i].IsActive(false);
            else
                shopUiObject[i].IsActive(true);
        }
    }

    #region Public Funcs

    public void SelectFish(SelectionInterface selected)
    {
        selectedID = selected.ID;

        fishToSellText.text = $"Trade {fishStocks[selectedID].count} X\n" +
                              $"{fishStocks[selectedID].fishName}\n For";

        fishToSellPrice.text = (FishDataManager.Instance.GetValue(selectedID) * fishStocks[selectedID].count).ToString();

        foreach (var UI in shopUiObject)
            UI.IsActive(false);

        confirmGameObject.SetActive(true);
    }

    public void ConfirmToSell()
    {
        sold.Post(gameObject);

        GameManager.Instance.fishCoin += (int) FishDataManager.Instance.GetValue(selectedID) * fishStocks[selectedID].count;

        InventoryManager.Instance.RemoveByType(selectedID);

        confirmGameObject.SetActive(false);

        UpdateFishShopText();
    }

    public void RejectToSell()
    {
        confirmGameObject.SetActive(false);

        DisableInteractIfNoFishStock();
    }

    #endregion

    private void OnDestroy()
    {
        EventManager.Instance.OnFishCaught -= UpdateFishShopText;
    }
}
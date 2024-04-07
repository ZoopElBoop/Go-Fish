using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishSellShopManager : MonoBehaviour
{
    public List<FishStoredData> fishStocks = new();

    public TMP_Text fishStockText;
    public TMP_Text fishCoinText;
    public GameObject sellButton;

    bool canSell = true;

    void Start()
    {
        UpdateFishShopText();

        EventManager.Instance.OnfishCaught += UpdateFishShopText;
    }

    private void Update()
    {
        if (!sellButton.activeSelf && canSell)
        {
            canSell = false;

            print("aaa");
            StartCoroutine(resetButton());
            SellFish();
        }
    }

    public void SellFish()
    {
        for (int i = 0; i < fishStocks.Count; i++)
        {
            if (fishStocks[i].count > 0)
                GameManager.Instance.fishCoin += FishDataManager.Instance.GetValue(i) * fishStocks[i].count;
        }

        InventoryManager.Instance.RemoveAll();
        
        UpdateFishShopText();
    }

    IEnumerator resetButton()
    {
        yield return new WaitForSeconds(0.15f);
        sellButton.SetActive(true);
        canSell = true;
    }

    private void UpdateFishShopText() 
    {
        print("RESET");

        fishStocks = InventoryManager.Instance.TotalStored();

        string fishStockedString = "Fish:\n";

        for (int i = 0; i < fishStocks.Count; i++)
        {
            fishStockedString += $"{fishStocks[i].count} {fishStocks[i].fishName}\n";
        }

        fishStockText.text = fishStockedString;

        fishCoinText.text = $"{GameManager.Instance.fishCoin}\nFish Coin";
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnfishCaught -= UpdateFishShopText;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishSellShopManager : MonoBehaviour
{
    List<FishStoredData> playerStocks = new();
    List<FishStoredData> boatStocks = new();
    List<FishStoredData> subStocks = new();

    public TMP_Dropdown dropdown;
    public TMP_Text text;
    public TMP_Text fishCoinText;
    public GameObject shopUi;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (shopUi.activeSelf)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                shopUi.SetActive(false);
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                shopUi.SetActive(true);
            }
        }
    }

    void Start()
    {
        shopUi.SetActive(false);

        playerStocks = InventoryManager.Instance.GetFromPlayer();
        boatStocks = InventoryManager.Instance.GetFromPlayer();
        subStocks = InventoryManager.Instance.GetFromPlayer();

        for (int i = 0; i < playerStocks.Count; i++)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(playerStocks[i].fishName));
           // print(playerStocks[i].fishName);
        }

        SelectToSell();
    }

    public void SelectToSell()
    {
        text.text = $"{playerStocks[dropdown.value].count} Of {playerStocks[dropdown.value].fishName}";
    }

    public void SellFish()
    {
        print("buton ");

        if (playerStocks[dropdown.value].count > 0)       
            InventoryManager.Instance.RemoveFromPlayer(dropdown.value);
        else
            return;

        GameManager.Instance.fishCoin += FishDataManager.Instance.GetValue(dropdown.value);

        UpdateStocks();
        SelectToSell();

        fishCoinText.text = $"FISHCOIN {GameManager.Instance.fishCoin}";
    }

    private void UpdateStocks() 
    {
        playerStocks = InventoryManager.Instance.GetFromPlayer();
        boatStocks = InventoryManager.Instance.GetFromPlayer();
        subStocks = InventoryManager.Instance.GetFromPlayer();
    }
}

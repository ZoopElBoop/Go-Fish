using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeShopManager : MonoBehaviour
{
    private ApplyUpgrades upgrades;

    public List<UpgradeData> upgradeTypes = new();
    public SelectionInterface[] shopUiObject;

    [Header("UI Elements")]
    public GameObject confirmGameObject;

    public TMP_Text UpgradeToShowText;
    public TMP_Text UpgradeToShowPrice;

    public TMP_Text fishCoinText;
    public GameObject hideCoin;

    private int selectedID = 0;

    public bool isBuying = true; //shop is either in buying mode or in equiping mode
    public UpgradeState[] upgradeStates;

    [Header("Slots Used")]
    public int playerSlots = 0;
    public int BoatSlots = 0;
    public int SubSlots = 0;

    [Header("Max Upgrade Slots")]
    public int playerSlotsMax;
    public int BoatSlotsMax;
    public int SubSlotsMax;

    //For some reason if i have one of these values set to 0 (default) i wont to able to compare it to anything (always returns true)
    //WHY????????????????????????
    //so this is why it be like this, values initialised in start as base value is literally nothing
    public enum UpgradeState
    {
        none = 1,
        bought = 2,
        equiped = 4
    }

    void Start()
    {
        confirmGameObject.SetActive(false);

        upgrades = GetComponent<ApplyUpgrades>();

        upgradeStates = new UpgradeState[upgradeTypes.Count];

        for (int i = 0; i < upgradeStates.Length; i++)  
            upgradeStates[i] = UpgradeState.none;

        UpgradeShopTextInit();

        UpdateUpgradeShopText();
    }

    #region Shop Ui 

    private void UpgradeShopTextInit()
    {
        for (int i = 0; i < shopUiObject.Length; i++)
        {
            if (shopUiObject[i].Name == null)
                Debug.LogError($"{shopUiObject[i].name} missing upgrade name text!!!");

            if (shopUiObject[i].Image == null)
                Debug.LogError($"{shopUiObject[i].name} missing upgrade image!!!");

            shopUiObject[i].ID = i;
        }

        if (shopUiObject.Length < upgradeTypes.Count)
            Debug.LogWarning("Not Enough Ui elements for all fish!!!");
    }

    private void UpdateUpgradeShopText()
    {
        if (isBuying)
        {
            for (int i = 0; i < upgradeTypes.Count; i++)
            {
                shopUiObject[i].Name.text = upgradeTypes[i].name;
                shopUiObject[i].Count.text = $"{upgradeTypes[i].cost}";
                shopUiObject[i].Image.sprite = upgradeTypes[i].image;
            }
        }
        else
        {
            for (int i = 0; i < upgradeTypes.Count; i++)
            {
                shopUiObject[i].Name.text = upgradeTypes[i].name;
                shopUiObject[i].Count.text = upgradeStates[i].ToString();
                shopUiObject[i].Image.sprite = upgradeTypes[i].image;
            }
        }

        fishCoinText.text = $"{GameManager.Instance.fishCoin}";

        DisableInteractIfNoUpgradesToShow();
    }

    private void DisableInteractIfNoUpgradesToShow()
    {
        if (isBuying)
        {
            for (int i = 0; i < upgradeTypes.Count; i++)
            {
                if (upgradeStates[i].HasFlag(UpgradeState.none) && GameManager.Instance.fishCoin >= upgradeTypes[i].cost)
                    shopUiObject[i].IsActive(true);
                else
                    shopUiObject[i].IsActive(false);
            }
        }
        else
        {
            for (int i = 0; i < upgradeTypes.Count; i++)
            {
                if (upgradeStates[i].HasFlag(UpgradeState.bought) || upgradeStates[i].HasFlag(UpgradeState.equiped))
                    shopUiObject[i].IsActive(true);
                else
                    shopUiObject[i].IsActive(false);
            }
        }
    }

    #endregion

    #region Upgrade Slots

    private bool CheckIfSlotsFree(int index)
    {
        switch (upgradeTypes[index].upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                return playerSlots + upgradeTypes[index].slotsTaken <= playerSlotsMax;
            case UpgradeData.UpgradeTo.Boat:
                return BoatSlots + upgradeTypes[index].slotsTaken <= BoatSlotsMax;
            case UpgradeData.UpgradeTo.Sub:
                return SubSlots + upgradeTypes[index].slotsTaken <= SubSlotsMax;
            case UpgradeData.UpgradeTo.Harpoon:
                break;
        }

        return true;
    }

    private void UpgradeToSlot(int i, bool apply)
    {
        int a = 1;

        if (!apply)
            a = -1;

        switch (upgradeTypes[i].upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                playerSlots += upgradeTypes[i].slotsTaken * a;
                break;
            case UpgradeData.UpgradeTo.Boat:
                BoatSlots += upgradeTypes[i].slotsTaken * a;
                break;
            case UpgradeData.UpgradeTo.Sub:
                SubSlots += upgradeTypes[i].slotsTaken * a;
                break;
            case UpgradeData.UpgradeTo.Harpoon:
                break;
        }
    }

    #endregion

    #region Public Funcs

    //Switch between buying or equiping
    //Update fish coin text to slots
    public void SwitchStates()
    {
        isBuying = !isBuying;

        UpdateUpgradeShopText();
    }

    public void SelectUpgrade(SelectionInterface selected)
    {
        selectedID = selected.ID;

        if (isBuying)
        {
            UpgradeToShowText.text = $"Buy \n{upgradeTypes[selectedID].name}\nFor";

            UpgradeToShowPrice.text = upgradeTypes[selectedID].cost.ToString();

            hideCoin.SetActive(true);
        }
        else 
        {
            if (upgradeStates[selectedID] == UpgradeState.equiped)
                UpgradeToShowText.text = $"Unequip \n{upgradeTypes[selectedID].name}\n";
            else
                UpgradeToShowText.text = $"Equip \n{upgradeTypes[selectedID].name}\n";

            UpgradeToShowPrice.text = null;

            hideCoin.SetActive(false);

        }

        foreach (var UI in shopUiObject)
            UI.IsActive(false);

        confirmGameObject.SetActive(true);
        print("selected");
    }

    public void AcceptChoice()
    {
        if (isBuying)
            ConfirmToSell();
        else
        {
            if (upgradeStates[selectedID].HasFlag(UpgradeState.bought))
                EquipUpgrade();
            else
                UnequipUpgrade();
        }

        confirmGameObject.SetActive(false);

        UpdateUpgradeShopText();
    }

    public void RejectChoice()
    {
        confirmGameObject.SetActive(false);

        DisableInteractIfNoUpgradesToShow();
    }

    #endregion

    #region Shop Events

    private void ConfirmToSell()
    {
        GameManager.Instance.fishCoin -= upgradeTypes[selectedID].cost;

        upgradeStates[selectedID] = UpgradeState.bought;

        print("bought");
    }

    private void EquipUpgrade()
    {

        upgradeStates[selectedID] = UpgradeState.equiped;

        UpgradeToSlot(selectedID, true);

        upgrades.Add(upgradeTypes[selectedID]);
    }

    private void UnequipUpgrade()
    {
        upgradeStates[selectedID] = UpgradeState.bought;

        UpgradeToSlot(selectedID, false);

        upgrades.Remove(upgradeTypes[selectedID]);
    }

    #endregion
}
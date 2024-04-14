using UnityEngine;

public class ApplyUpgrades : MonoBehaviour
{
    private Fishing fis;
    public Boat boatScript;

    private void Start()
    {
        fis = PlayerScriptManager.Instance.GetScript("Fishing");
    }

    public void Add(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                fis._bobberMaxDistanceFromPlayer += upgrade.bobberRangeAddon;
                break;
            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed *= upgrade.speedMulti;
                break;
            case UpgradeData.UpgradeTo.Sub:
                break;
            case UpgradeData.UpgradeTo.Harpoon:
                break;
            default:
                break;
        }
    }

    public void Remove(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                fis._bobberMaxDistanceFromPlayer -= upgrade.bobberRangeAddon;
                break;
            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed = 30;
                break;
            case UpgradeData.UpgradeTo.Sub:
                break;
            case UpgradeData.UpgradeTo.Harpoon:
                break;
            default:
                break;
        }
    }
}

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
                fis._chargeUpMax += upgrade.rodThrowAddon;
                fis._bobberMaxDistanceFromPlayer += upgrade.bobberRangeAddon;
                break;

            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed += upgrade.speedAddon;
                boatScript.stealthMultiplier -= upgrade.stealthMulti;
                break;

            case UpgradeData.UpgradeTo.Sub:
                //TBD
                break;

            case UpgradeData.UpgradeTo.Harpoon:
                //TBD
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
                fis._chargeUpMax -= upgrade.rodThrowAddon;
                fis._bobberMaxDistanceFromPlayer -= upgrade.bobberRangeAddon;
                break;

            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed -= upgrade.speedAddon;
                boatScript.stealthMultiplier += upgrade.stealthMulti;
                break;

            case UpgradeData.UpgradeTo.Sub:
                //TBD
                break;

            case UpgradeData.UpgradeTo.Harpoon:
                //TBD
                break;

            default:
                break;
        }
    }
}

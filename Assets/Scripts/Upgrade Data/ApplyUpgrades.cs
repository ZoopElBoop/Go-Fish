using UnityEngine;

public class ApplyUpgrades : MonoBehaviour
{
    private Fishing fis;
    public Boat boatScript;
    public GameObject sublight;

    private void Start()
    {
        fis = PlayerScriptManager.Instance.GetScript("Fishing");
    }

    public void Add(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                break;

            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed += upgrade.speedAddon;
                boatScript.stealthMultiplier -= upgrade.stealthMulti;
                break;

            case UpgradeData.UpgradeTo.Sub:
                sublight.SetActive(true);
                break;

            case UpgradeData.UpgradeTo.Harpoon:
                GameManager.Instance.harpoonSpeed =+ upgrade.harpoonSpeed;
                GameManager.Instance.harpoonDamage =+ upgrade.harpoonDamage;
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
                break;

            case UpgradeData.UpgradeTo.Boat:
                boatScript._Speed -= upgrade.speedAddon;
                boatScript.stealthMultiplier += upgrade.stealthMulti;
                break;

            case UpgradeData.UpgradeTo.Sub:
                sublight.SetActive(true);
                break;

            case UpgradeData.UpgradeTo.Harpoon:
                GameManager.Instance.harpoonSpeed =+ upgrade.harpoonSpeed;
                GameManager.Instance.harpoonDamage =+ upgrade.harpoonDamage;
                break;

            default:
                break;
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade Data Block", menuName = "ScriptableObjects/Upgrade Data Object")]
public class UpgradeData : ScriptableObject
{
    [Space]

    public UpgradeTo upgradeType;
    public enum UpgradeTo
    {
        Player,
        Boat,
        Sub,
        Harpoon
    }

    [Space]

    [Min(1)]public int cost;
    [Range (1,2)]public int slotsTaken = 1;

    public Sprite image;
    [Multiline] public string description;

    #region Upgrade Variables

    [Header("Player")]
    [Range(0, 100)] public int rodThrowAddon;
    [Range(0, 100)] public int bobberRangeAddon;

    [Header("Boat")]
    [Range(0, 100)] public int speedAddon;
    [Range(0f, 1f)] public float stealthMulti;

    [Header("Sub")]
    public string addonType;

    [Header("Harpoon")]
    public GameObject harpoon;
    [Range(1, 100)] public int harpoonDamage;
    [Range(1, 100)] public int harpoonSpeed;

    #endregion

    private string previousType;

    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 
    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 
    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 

    //Ok its not as bad now but i still hate this
    private void OnValidate()
    {
        previousType ??= upgradeType.ToString();    //checks if previous type is null, to prevent values changing on editor startup                           

        if (previousType != upgradeType.ToString())
        {
            previousType = upgradeType.ToString();

            //player reset
            rodThrowAddon = 0;
            bobberRangeAddon = 0;

            //boat reset
            speedAddon = 0;
            stealthMulti = 0f;

            //sub reset
            addonType = null;

            //harpoon reset
            harpoon = null;
            harpoonDamage = 1;
            harpoonSpeed = 1;

            //Removes slots value if harpoon upgrade type
            if (previousType == "Harpoon")
                slotsTaken = 0;
            else
                slotsTaken = 1;
        }
    }
}

#region Hide values in edior

#if UNITY_EDITOR

[CustomEditor(typeof(UpgradeData))]
class ValueVisible : Editor
{
    private List<string> ignoreValues = new();

    public override void OnInspectorGUI()
    {
        UpgradeData self = (UpgradeData)target;
        serializedObject.Update();

        ignoreValues.Clear();

        switch (self.upgradeType)
        {
            case UpgradeData.UpgradeTo.Player:
                HideBoatData();
                HideSubData();
                HideHarpoonData();
                break;

            case UpgradeData.UpgradeTo.Boat:
                HidePlayerData();
                HideSubData();
                HideHarpoonData();
                break;

            case UpgradeData.UpgradeTo.Sub:
                HidePlayerData();
                HideBoatData();
                HideHarpoonData();
                break;
            case UpgradeData.UpgradeTo.Harpoon:
                HidePlayerData();
                HideBoatData();
                HideSubData();
                HideBaseData();
                break;
        }

        DrawPropertiesExcluding(serializedObject, ignoreValues.ToArray());
        serializedObject.ApplyModifiedProperties();
    }

    private void HidePlayerData() 
    {
        ignoreValues.Add(nameof(UpgradeData.rodThrowAddon));
        ignoreValues.Add(nameof(UpgradeData.bobberRangeAddon));
    }
    private void HideBoatData()
    {
        ignoreValues.Add(nameof(UpgradeData.speedAddon));
        ignoreValues.Add(nameof(UpgradeData.stealthMulti));
    }
    private void HideSubData()
    {
        ignoreValues.Add(nameof(UpgradeData.addonType));
    }
    private void HideHarpoonData()
    {
        ignoreValues.Add(nameof(UpgradeData.harpoon));
        ignoreValues.Add(nameof(UpgradeData.harpoonDamage));
        ignoreValues.Add(nameof(UpgradeData.harpoonSpeed));
    }
    private void HideBaseData() 
    {
        ignoreValues.Add(nameof(UpgradeData.slotsTaken));
    }
}

#endif

#endregion
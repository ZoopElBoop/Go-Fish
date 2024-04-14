using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade Data Block", menuName = "ScriptableObjects/Upgrade Data Object")]
public class UpgradeData : ScriptableObject
{
    [Header("This is the most jank and stupid way of doing this but i am very tired sorry :(")]
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

    public int rodThrowAddon;
    public int bobberRangeAddon;

    [Header("Boat")]
    public float speedMulti;
    public float stealthMulti;

    [Header("Sub")]
    public string addonType;

    [Header("Harpoon")]
    public GameObject harpoon;
    public float harpoonDamageMulti;
    public float harpoonSpeedMulti;


    private string previousType;

    #endregion

    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 
    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 
    //I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS I HATE THIS 
    private void OnValidate()
    {
        if (previousType != upgradeType.ToString())
        {
            previousType = upgradeType.ToString();

            //player reset
            rodThrowAddon = 0;
            bobberRangeAddon = 0;

            //boat reset
            speedMulti = 0f;
            stealthMulti = 0f;

            //sub reset
            addonType = null;

            //harpoon reset
            harpoon = null;
            harpoonDamageMulti = 0f;
            harpoonSpeedMulti = 0f;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(UpgradeData))]
class ValueVisible : Editor
{
    List<string> ignoreValues = new();

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
        ignoreValues.Add(nameof(UpgradeData.speedMulti));
        ignoreValues.Add(nameof(UpgradeData.stealthMulti));
    }
    private void HideSubData()
    {
        ignoreValues.Add(nameof(UpgradeData.addonType));
    }
    private void HideHarpoonData()
    {
        ignoreValues.Add(nameof(UpgradeData.harpoon));
        ignoreValues.Add(nameof(UpgradeData.harpoonDamageMulti));
        ignoreValues.Add(nameof(UpgradeData.harpoonSpeedMulti));
    }

    private void HideBaseData() 
    {
        ignoreValues.Add(nameof(UpgradeData.slotsTaken));
    }
}

#endif
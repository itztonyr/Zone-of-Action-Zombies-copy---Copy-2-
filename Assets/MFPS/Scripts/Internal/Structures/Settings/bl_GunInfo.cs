using MFPS.Internal.Structures;
using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class bl_GunInfo
{
    [Header("Info")]
    /// <summary>
    /// Display name of this weapon
    // </summary>
    public string Name;
    /// <summary>
    /// Internal name of this weapon, use to identify this weapon in case the Gun ID change.
    /// </summary>
    [HideInInspector] public string Key;
    /// <summary>
    /// The weapon type of this weapon.
    /// </summary>
    public GunType Type = GunType.Machinegun;
    /// <summary>
    /// Since removing the weapon from GameData cause a lot of problems, this is the alternative way to disable a weapon.
    /// </summary>
    [LovattoToogle] public bool Active = true;

    [Header("Settings")]
    [Range(1, 100)] public int Damage;
    [Range(0.01f, 2f)] public float FireRate = 0.1f;
    [Range(0.5f, 10)] public float ReloadTime = 2.5f;
    [Range(0, 1000)] public int Range;
    [Range(0.01f, 5)] public float Accuracy;
    [Range(0, 4)] public float Weight;
    public MFPSItemUnlockability Unlockability;

    [Header("References")]
    [SpritePreview(30, true)] public Sprite GunIcon;

    // define how much each specification should influence the overall power of a weapon
    private readonly Dictionary<string, float> weights = new()
        {
            {"Damage", 0.25f},
            {"FireRate", 0.20f},
            {"Accuracy", 0.20f},
            {"Weight", 0.10f},
            {"ReloadTime", 0.15f},
            {"Range", 0.10f}
        };

    /// <summary>
    /// Can show this weapons in the game lists like class customizer, customizer, unlocks, etc...
    /// </summary>
    /// <returns></returns>
    public bool CanShowWeapon()
    {
        return Active && Unlockability.UnlockMethod != MFPSItemUnlockability.UnlockabilityMethod.Hidden;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weights"></param>
    /// <returns></returns>
    public float CalculatePower()
    {
        float normalizedDamage = Damage / 100;
        float normalizedFireRate = FireRate / 1;
        float normalizedAccuracy = Accuracy / 5;
        float normalizedWeight = Weight / 5;
        float normalizedReloadTime = ReloadTime / 7;
        float normalizedRange = Range / 1000;

        float power = (normalizedDamage * weights["Damage"]) +
                      (normalizedFireRate * weights["FireRate"]) +
                      (normalizedAccuracy * weights["Accuracy"]) +
                      ((1 - normalizedWeight) * weights["Weight"]) + // lower weight is better
                      ((1 - normalizedReloadTime) * weights["ReloadTime"]) +
                      (normalizedRange * weights["Range"]);

        return power;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkManager", menuName = "Zombies/PerkManager", order = 1)]
public class bl_PerkManager : ScriptableObject
{
    public int MaxPerks = 4;
    [HideInInspector] public int CurrentPerkAmount;

    [Header("Jugernog Ability Settings")]
    [Space(5)]
    public int newHealth;
    public int newHealthMax;
    public int newRegenUpTo;
    [Header("Double Tap Ability Settings")]
    [Space(5)]
    public float newFireRate;
    [Header("SpeedCola Ability Settings")]
    [Space(5)]
    public float newReloadSpeed;
    [Header("Stamina Up ability Settings")]
    [Space(5)]
    public float newWalkSpeed;
    public float newRunSpeed;
    [Header("Perk Interaction Settings")]
    [Space(5)]
    public float interactRange = 3f;
    public float disambleweaponsfor = 5f;
    public KeyCode interactionKey = KeyCode.F;



    [HideInInspector] public bool canBuyH = true;
    [HideInInspector] public bool canBuyF = true;
    [HideInInspector] public bool canBuyS = true;
    [HideInInspector] public bool canBuyM = true;
    [HideInInspector] public bool canBuyR = true;
    [HideInInspector] public bool isBuyH = false;
    [HideInInspector] public bool isBuyF = false;
    [HideInInspector] public bool isBuyS = false;
    [HideInInspector] public bool isBuyM = false;
    [HideInInspector] public bool isBuyR = false;
}

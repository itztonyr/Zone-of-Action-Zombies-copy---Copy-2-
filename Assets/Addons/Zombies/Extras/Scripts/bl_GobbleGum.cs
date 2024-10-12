using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_GobbleGum : MonoBehaviour
{
    [Header("Gobble Gum Interaction Settings")]
    [Space(5)]
    public float interactRange = 3f;
    public KeyCode interactionKey = KeyCode.F;

    [Header("Gobble Gum Settings")]
    [Space(5)]
    public int MaxActiveGobbleGums = 4;
    public bl_GobbleGumLoadout loadout;
    [HideInInspector] public int CurrentGobbleGumAmount;



}

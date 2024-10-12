using UnityEngine;
using MFPS.Internal.Structures;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "GobbleGum", menuName = "Zombies/GobbleGum", order = 1)]
public class bl_GobbleGumSingle : ScriptableObject
{
    public enum GobbleGumType
    {
       Speed = 0,
       Health = 1,
       Damage = 2,
       Regeneration = 3,
       Weapon = 4,
    }
    public string Name = "CrackHead";
    public GobbleGumType Type;
    [Header("Health")]
    [Space(5)]
    [Range(1, 1000)] public int newHealth = 100;
    [Range(1, 1000)] public int newHealthMax = 100;
    [Range (0, 10)] public int newRegenSpeed = 75;
    [Header("Speed")]
    [Space(5)]
    [Range(1, 20)] public float newWalkSpeed = 1f;
    [Range(1, 40)] public float newRunSpeed = 1f;
    [Header("Weapon")]
    [Space(5)]
    [Range(0.01f, 1)] public float newFireRate = 1f;
    [Range(0.01f, 10)] public float newReloadSpeed = 1f;
    [Header("Damage")]
    [Space(5)]
    [Range(1, 1000)] public int newGunDamage = 100;
    [Space(10)]
    public float Duration = -1f; //-1 means the gobblegum will last forever
    [Space(10)]
    public MFPSItemUnlockability Unlockability;

  

    public void DisambleEffects()
    {
        foreach(bl_GobbleGumSingle gob in bl_GobbleGumLoadout.Instance.gobblegums)
        {
            gob.DisambleEffect();
        }
    }
    public void DisambleEffect()
    {

    }
    public void ActivateEffect()
    {
        var PlayerRef = bl_GameManager.Instance.LocalPlayerReferences;

        if (PlayerRef != null)
        {
            if (Type == GobbleGumType.Speed)
            {
                PlayerRef.firstPersonController.MFPSController.WalkSpeed = newWalkSpeed;
                PlayerRef.firstPersonController.MFPSController.runSpeed = newRunSpeed;

            }
            if (Type == GobbleGumType.Health)
            {
                PlayerRef.gameObject.GetComponent<bl_PlayerHealthManager>().health = newHealth;
                PlayerRef.gameObject.GetComponent<bl_PlayerHealthManager>().maxHealth = newHealthMax;
            }
            if (Type == GobbleGumType.Damage)
            {
                foreach (bl_Gun gun in PlayerRef.gunManager.PlayerEquip)
                {
                    gun.extraDamage = newGunDamage;
                }
            }
            if (Type == GobbleGumType.Regeneration)
            {
                PlayerRef.gameObject.GetComponent<bl_PlayerHealthManager>().RegenerateUpTo = PlayerRef.gameObject.GetComponent<bl_PlayerHealthManager>().maxHealth;
                PlayerRef.gameObject.GetComponent<bl_PlayerHealthManager>().RegenerationSpeed = newRegenSpeed;
            }
            if (Type == GobbleGumType.Weapon)
            {
                foreach (bl_Gun gun in PlayerRef.gunManager.PlayerEquip)
                {
                    gun.extraFireRate = -newFireRate;
                    gun.extraReloadTime = -newReloadSpeed;
                }
            }
        }
    }

}

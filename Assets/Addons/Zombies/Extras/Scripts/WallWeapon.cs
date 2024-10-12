using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ZombiesGameMode;
using UnityEngine.UI;
using MFPS.Internal.Structures;

public class WallWeapon : MonoBehaviour
{
    #region Public
    [Header("Settings")]
    [Space(5)]
    public WeaponType Type;
    [GunID] public int Weapon;
    public float interactionRange = 5f;
    public int cost = 950;
    public TextMeshProUGUI GunText;
    [Header("Key Setings")]
    [Space(5)]
    public KeyCode interactionKey = KeyCode.F;
    [Header("Audio Setings")]
    [Space(5)]
    public AudioClip PurchaseSound;
    [Header("Weapon Ammo Settings")]
    [Space(5)]
    public int AmmoCost = 250;
    public int BulletsOnAmmoPurchase = 30;
    public int ProjectilesOnAmmoPurchase = 2;
    #endregion

    #region Private
    private bl_PlayerReferences references;
    private bl_GunManager GunManager;
    [HideInInspector] public List<bl_Gun> AllGuns;
    [HideInInspector] public bl_Gun gun;
    private bool isBought = false;
    private bool isInRange = false;
    private bool canUse;
    private bool canUseAmmo;
    private bool wasUsed;
    private bl_RoundManager roundManager;
    private bl_Gun boughtGun;
    private int currentScore;
    private Image image;
 
    #endregion

    #region Unity
    void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }
    private void Start()
    {
        image = GetComponentInChildren<Image>();
        GunText = GetComponentInChildren<TextMeshProUGUI>();
        if (image != null)
        {
            image.gameObject.SetActive(false);
        }
        GunText.gameObject.SetActive(false);
        wasUsed = false;
        roundManager = FindObjectOfType<bl_RoundManager>();
    }
    private void OnLocalPlayerSpawn()
    {
        references = bl_GameManager.Instance.LocalPlayerReferences;
        GunManager = references.gunManager;
    }
    void Update()
    {
        List<bl_Gun> playerEquip = GunManager.PlayerEquip;
        gun = GunManager.CurrentGun;
        AllGuns = new List<bl_Gun>(playerEquip);

        if (bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.position == null) return;

        isInRange = Vector3.Distance(transform.position, bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.position) <= interactionRange;
        canUse = isInRange && roundManager.CanAfford(cost) && !isBought && Input.GetKeyDown(interactionKey);
        canUseAmmo = isInRange && roundManager.CanAfford(AmmoCost) && isBought && Input.GetKeyDown(interactionKey);
        boughtGun = GunManager.GetGunOnListById(Weapon);
        if (isInRange && !isBought)
        {
            if (playerEquip.Contains(boughtGun))
            {
                wasUsed = true;
                canUse = false;
                isBought = true;

                if ((boughtGun.maxNumberOfClips * boughtGun.bulletsPerClip) >= boughtGun.bulletsLeft)
                {
                    if (image != null)
                    {
                        image.gameObject.SetActive(false);
                    }
                    GunText.gameObject.SetActive(false);
                    GunText.text = "BUY AMMO FOR: " + boughtGun.name.ToString() + " COSTS " + AmmoCost.ToString();
                }
                else
                {

                    if (image != null)
                    {
                        image.gameObject.SetActive(isInRange);
                    }
                    GunText.gameObject.SetActive(isInRange);
                    GunText.text = "BUY AMMO FOR: " + boughtGun.name.ToString() + " COSTS " + AmmoCost.ToString();
                }
            }
            else
            {
                if (image != null)
                {
                    image.gameObject.SetActive(isInRange);
                }
                GunText.gameObject.SetActive(isInRange);
                GunText.text = boughtGun.name.ToString() + " COSTS " + cost.ToString();
            }
        }
        else
        {
            if (image != null)
            {
                image.gameObject.SetActive(isInRange);
            }
            GunText.gameObject.SetActive(isInRange);
        }
      
        if (isInRange && isBought && gun.GunID == Weapon)
        {
            if (image != null)
            {
                image.gameObject.SetActive(isInRange);
            }
            GunText.gameObject.SetActive(isInRange);
            GunText.text = "BUY AMMO FOR: " + boughtGun.name.ToString() + " COSTS " + AmmoCost.ToString();
        }
        else
        {
            if (image != null)
            {
                image.gameObject.SetActive(isInRange);
            }
            GunText.gameObject.SetActive(isInRange);
        }
        
        if (canUse && !wasUsed) //first buy 
        {
            if (roundManager.playerScore <= cost)
            {
                ShowNotificationSample2();
                return;
            }
            wasUsed = true;
            canUse = false;
            isBought = true;
            if (PurchaseSound != null)
            {
                AudioSource.PlayClipAtPoint(PurchaseSound, transform.position);
            }
            ReduceScore(cost);
            if (Type == WeaponType.Primary)
            {
                GunManager.AddWeaponToSlot(0, GunManager.GetGunOnListById(Weapon), true);
            }
            if (Type == WeaponType.Secondary)
            {
                GunManager.AddWeaponToSlot(1, GunManager.GetGunOnListById(Weapon), true);
            }
            if (Type == WeaponType.Lethal)
            {
                GunManager.AddWeaponToSlot(2, GunManager.GetGunOnListById(Weapon), true);
            }
            if (Type == WeaponType.Meele)
            {
                GunManager.AddWeaponToSlot(3, GunManager.GetGunOnListById(Weapon), true);
            }
            if (Type == WeaponType.Custom)
            {
                GunManager.AddWeaponToSlot(4, GunManager.GetGunOnListById(Weapon), true); //creates a new slot for a special weapon maybe?
            }
        }
        if (canUseAmmo && wasUsed && gun.GunID == Weapon) //at this point we bought the weapon, soo now you can only get ammo from it
        {
            if (playerEquip.Contains(boughtGun))
            {
                if (roundManager.playerScore <= AmmoCost)
                {
                    ShowNotificationSample();
                    return;
                }

                if (PurchaseSound != null)
                {
                    AudioSource.PlayClipAtPoint(PurchaseSound, transform.position);
                }
                ReduceScore(AmmoCost);
                bl_EventHandler.OnAmmo(BulletsOnAmmoPurchase, ProjectilesOnAmmoPurchase, Weapon);
            }
        }
    }
    public void ReduceScore(int amount)
    {
        roundManager.playerScore -= cost;
        UpdateScoreDisplay();
    }
    void UpdateScoreDisplay()
    {
        currentScore = roundManager.playerScore;
        roundManager.IncreaseScore(0);
    }
    void ShowNotificationSample()
    {
        new MFPSLocalNotification("YOU DONT HAVE ENOUGH MONEY FOR AMMO");
    }
    void ShowNotificationSample2()
    {
        new MFPSLocalNotification("YOU DONT HAVE ENOUGH MONEY FOR A WEAPON!");
    }
    #endregion
}

public enum WeaponType
{
    Primary = 0,
    Secondary = 1,
    Lethal = 2,
    Meele = 3,
    Custom = 4,
}

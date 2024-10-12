using UnityEngine;
using TMPro;
using System.Collections.Generic;
using ZombiesGameMode;
using UnityEngine.UI;
using MFPS.Internal.Structures;
using Photon.Pun;

public class bl_Perk : MonoBehaviourPun
{
    #region Public
    [Header("Perk Settings")]
    [Space(5)]
    public string perkName = "Perk Name";
    public int cost = 500;
    public TextMeshProUGUI promptText;


    [Header("Perk Ability")]
    [Space(5)]
    public PerkType MachinesPerk;
    #endregion

    #region Private
    private int currentscore;
    private bl_GunManager GunManager;
    private bool isInRange;
    private GameObject player;
    private Transform playerTransform;
    private bl_RoundManager roundmanager;
    private bool canInteract;
    [HideInInspector] public List<bl_Gun> AllGuns;
    private int currentGunID;
    [HideInInspector] public bl_Gun gun;
    private bl_PlayerHealthManager playerhealth;
    private bl_ScenePerkManager SceneManager;
    private bl_FirstPersonController playercontroller;
    private bool isDrinkBought = false;
    private float oldtimer;
    private bl_PerkIconManager icon;
    private Image backround;
    #endregion


    private void Awake()
    {
        backround = GetComponentInChildren<Image>();
        promptText.text = "PERK: " + perkName.ToString() + "  COST: " + cost.ToString();
        roundmanager = FindObjectOfType<bl_RoundManager>();
        SceneManager = FindObjectOfType<bl_ScenePerkManager>();
        icon = FindObjectOfType<bl_PerkIconManager>();
        isDrinkBought = false;
        oldtimer = SceneManager.manager.disambleweaponsfor;

        icon.HPerkIcon.SetActive(false);
        icon.FPerkIcon.SetActive(false);
        icon.MPerkIcon.SetActive(false);
        icon.SPerkIcon.SetActive(false);
        icon.RPerkIcon.SetActive(false);

        foreach (bl_Gun allGun in AllGuns)
        {
            if (allGun != null)
            {
                for (int i = 0; i < AllGuns.Count; i++)
                {
                    allGun.extraFireRate = 0;
                    allGun.extraReloadTime = 0;
                    allGun.extraDamage = 0;
                }

            }
        }
        promptText.enabled = true;
    }
    private void Update()
    {
        if (player == null)
        {
            player = bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.gameObject;
            if(player.transform == null)
            {
                Debug.Log("We are most likely dead");
            }
        }
        else
        {
            Debug.Log("Player Not Found");
        }

        isInRange = Vector3.Distance(transform.position, bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.position) <= SceneManager.manager.interactRange;

        if (backround != null) {
            backround.gameObject.SetActive(isInRange);
        }

        CheckPlayerProximity();

        if (isDrinkBought == false && SceneManager.manager.canBuyF || SceneManager.manager.canBuyH || SceneManager.manager.canBuyM || SceneManager.manager.canBuyR || SceneManager.manager.canBuyS)
        {
            if (isDrinkBought)
                return;
            else
            {
                Drink();
            }
        }

        GunManager = bl_Zombies.Instance.LocalPlayerReferences.gunManager;
        playerhealth = bl_Zombies.Instance.LocalPlayerReferences.Transform.gameObject.GetComponent<bl_PlayerHealthManager>();
        playercontroller = bl_Zombies.Instance.LocalPlayerReferences.firstPersonController.MFPSController;

        // Access the PlayerEquip list from GunManager
        List<bl_Gun> playerEquip = GunManager.PlayerEquip;

        // Get the CurrentGun from GunManager
        gun = GunManager.CurrentGun;
        currentGunID = (gun != null) ? gun.GunID : -1;

        // Create a new list and assign the contents of playerEquip to it
        AllGuns = new List<bl_Gun>(playerEquip);


        if (SceneManager.manager.CurrentPerkAmount == SceneManager.manager.MaxPerks)
        {
            SceneManager.manager.canBuyF = false;
            SceneManager.manager.canBuyH = false;
            SceneManager.manager.canBuyS = false;
            SceneManager.manager.canBuyM = false;
            SceneManager.manager.canBuyR = false;
            canInteract = false;
        }

    }

    public bool CanAfford(int amount)
    {

        return roundmanager.playerScore >= amount;
    }

    public void ReduceScore(int amount)
    {
        roundmanager.playerScore -= cost;
        UpdateScoreDisplay();
    }

    public void IncreaseScore(int amount)
    {
        roundmanager.playerScore += cost;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        currentscore = roundmanager.playerScore;
    }
    void ShowNotificationSample()
    {
        new MFPSLocalNotification("YOU DONT HAVE ENOUGH MONEY FOR THE PERK!");
    }
    private void ModifyPlayer()
    {
        if (!photonView.IsMine) return;

        isDrinkBought = true;

        if (MachinesPerk == PerkType.Jugernog && SceneManager.manager.canBuyH && SceneManager.manager.isBuyH)
        {
            Debug.Log("ActionStared");
            Drink();
            SceneManager.manager.canBuyH = false;
            playerhealth.health = SceneManager.manager.newHealth;
            playerhealth.maxHealth = SceneManager.manager.newHealthMax;
            playerhealth.RegenerateUpTo = SceneManager.manager.newRegenUpTo;
            SceneManager.manager.CurrentPerkAmount++;
            icon.HPerkIcon.SetActive(true);
        }
        if (MachinesPerk == PerkType.DoubleTap && SceneManager.manager.canBuyF && SceneManager.manager.isBuyF)
        {
            Debug.Log("ActionStared1");
            Drink();
            SceneManager.manager.canBuyF = false;
            SceneManager.manager.CurrentPerkAmount++;

            foreach (bl_Gun allGun in AllGuns)
            {
                if (allGun != null)
                {
                    for (int i = 0; i < AllGuns.Count; i++)
                    {
                        Debug.Log("ExecuteFireRateChanges");
                        allGun.extraFireRate = SceneManager.manager.newFireRate;
                    }

                }
            }
            icon.FPerkIcon.SetActive(true);
        }
        if (MachinesPerk == PerkType.StaminaUp && SceneManager.manager.canBuyM && SceneManager.manager.isBuyM)
        {
            Debug.Log("ActionStared2");
            Drink();
            SceneManager.manager.canBuyM = false;
            SceneManager.manager.CurrentPerkAmount++;
            playercontroller.runSpeed = SceneManager.manager.newRunSpeed;
            playercontroller.WalkSpeed = SceneManager.manager.newWalkSpeed;
            icon.MPerkIcon.SetActive(true);
        }
        if (MachinesPerk == PerkType.SpeedCola && SceneManager.manager.canBuyS && SceneManager.manager.isBuyS)
        {
            Debug.Log("ActionStared3");
            Drink();
            SceneManager.manager.canBuyS = false;
            SceneManager.manager.CurrentPerkAmount++;
            foreach (bl_Gun allGun in AllGuns)
            {
                if (allGun != null)
                {
                    for (int i = 0; i < AllGuns.Count; i++)
                    {
                        allGun.extraReloadTime = SceneManager.manager.newReloadSpeed;
                    }

                }
            }
            icon.SPerkIcon.SetActive(true);
        }
        if (MachinesPerk == PerkType.QuickRevive && SceneManager.manager.canBuyR && SceneManager.manager.isBuyR)
        {
            Debug.Log("ActionStared4");
            Drink();
            SceneManager.manager.canBuyR = false;
            SceneManager.manager.CurrentPerkAmount++;
            icon.RPerkIcon.SetActive(true);
            playercontroller.OwnsRevive = true;
 
        }
        UpdateScoreDisplay();
    }

    private void CheckPlayerProximity()
    {
        if (isDrinkBought)
            return;
        if (player != null)
        {
            if (isInRange)
            {
                promptText.text = "PERK: " + perkName.ToString() + "  COST: " + cost.ToString();
                isInRange = true;
                canInteract = true;
            }
            else
            {
                canInteract = false;
            }
            if (canInteract && Input.GetKeyDown(SceneManager.manager.interactionKey))
            {
                AttemptToBuyPerk();
            }

            if (canInteract)
            {
                isInRange = true;
                backround.enabled = true;
                promptText.enabled = true;
            }
            else
            {
                backround.enabled = false;
                promptText.enabled = false;
                promptText.text = "";
            }
        }
        

        
    }

    public void AttemptToBuyPerk()
    {
        if (isDrinkBought)
            return;
        if (CanAfford(cost) && Input.GetKeyDown(SceneManager.manager.interactionKey))
        {
            ReduceScore(cost);
            if (MachinesPerk == PerkType.Jugernog)
            {
                SceneManager.manager.isBuyH = true;
            }
            if (MachinesPerk == PerkType.DoubleTap)
            {
                SceneManager.manager.isBuyF = true;
            }
            if (MachinesPerk == PerkType.SpeedCola)
            {
                SceneManager.manager.isBuyS = true;
            }
            if (MachinesPerk == PerkType.QuickRevive)
            {
                SceneManager.manager.isBuyR = true;
            }
            if (MachinesPerk == PerkType.StaminaUp)
            {
                SceneManager.manager.isBuyM = true;
            }
            ModifyPlayer();
            Debug.Log("Bought perk: " + perkName);
        }
        else
        {
            Debug.Log("Not enough score to buy: " + perkName);
            ShowNotificationSample();
        }
    }

    

    void Drink()
    {

        if (isDrinkBought == true)
        {

            if (SceneManager.manager.disambleweaponsfor > 0)
            {
                SceneManager.manager.disambleweaponsfor -= Time.deltaTime;
                GunManager.BlockAllWeapons();
                GunManager.Drinking();
            }
            if (SceneManager.manager.disambleweaponsfor <= 0)
            {
                GunManager.ReleaseWeapons(false);
                GunManager.StopDrink();
                isDrinkBought = false;
                SceneManager.manager.disambleweaponsfor = oldtimer;
            }
        }
       
    }
    public enum PerkType
    {
        QuickRevive,
        DoubleTap,
        SpeedCola,
        Jugernog,
        StaminaUp,
    }
}

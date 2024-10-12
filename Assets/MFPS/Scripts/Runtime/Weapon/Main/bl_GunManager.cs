using MFPS.Internal.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_GunManager : bl_MonoBehaviour
{
    #region Public members
    [Header("Weapons List")]
    public bl_WeaponContainer weaponContainer;
    public List<bl_Gun> AllGuns = new();

    /// <summary>
    /// The equipped weapons of this player
    /// By default only 4 weapons are equipped.
    /// </summary>
    public List<bl_Gun> PlayerEquip { get; set; } = new List<bl_Gun>();

    [Header("Player Class")]
    public bl_PlayerClassLoadout m_AssaultClass;
    public bl_PlayerClassLoadout m_EngineerClass;
    public bl_PlayerClassLoadout m_ReconClass;
    public bl_PlayerClassLoadout m_SupportClass;

    [Header("Settings")]
    /// <summary>
    /// ID the weapon to take at start
    /// </summary>
    public int currentWeaponIndex = 0;
    /// <summary>
    /// time it takes to switch weapons
    /// </summary>
    public float SwichTime = 1;
    public float PickUpTime = 2.5f;
    public ChangeWeaponStyle changeWeaponStyle = ChangeWeaponStyle.HideAndDraw;

    [Header("References")]
    public Animator HeadAnimator;
    #endregion

	public AudioClip DrinkAudioClip;
    public Animator animatorDrink;
    public bool useHideAnimationDrink;
    public GameObject Drink;

    #region Public properties
    /// <summary>
    /// The current equipped gun of this player
    /// </summary>
    public bl_Gun CurrentGun
    {
        get
        {
            return PlayerEquip == null || PlayerEquip.Count == 0 ? null : PlayerEquip[currentWeaponIndex];
        }
    }

    /// <summary>
    /// Can the player change of equipped weapon?
    /// </summary>
    public bool CanSwich
    {
        get;
        set;
    } = true;

    /// <summary>
    /// 
    /// </summary>
    public bool IsGameStarted
    {
        get;
        set;
    }

    /// <summary>
    /// Equipped player loadout data
    /// </summary>
    public bl_PlayerClassLoadout CurrentLoadout { get; set; }

    /// <summary>
    /// Cache option from GameData
    /// </summary>
    public AutoChangeOnPickup EquipPickUpMode { get; set; } = AutoChangeOnPickup.Always;
    #endregion

    #region Private members
    private int PreviousGun = 0;
    private bool isFastFire = false;
    public bool ObservedComponentsFoldoutOpen = false;
    public GameObject _editorWeaponContainerInstance = null;
    private AudioSource fireAudioSource;
    private readonly int meleeSlot = 3;
    private readonly int lethalSlot = 2;
    #endregion

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        IsGameStarted = bl_MatchTimeManagerBase.HaveTimeStarted();

        //when player instance select player class select in bl_RoomMenu
        SetupLoadout();
        //setup the equipped weapons
        SetupEquippedWeapons();

        bl_EventHandler.Player.onLocalWeaponLoadoutReady?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (bl_GameManager.Instance != null && bl_GameManager.Instance.GameMatchState == MatchState.Waiting && !bl_GameManager.Instance.FirstSpawnDone)
        {
            BlockAllWeapons();
        }

#if LMS
        if (GetGameMode == GameMode.BR)
        {
            UnEquipEverything();
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPickUpGun += this.PickUpGun;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPickUpGun -= this.PickUpGun;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!bl_GameInput.IsCursorLocked)
            return;

        InputControl();
    }
    #endregion

    /// <summary>
    /// Setup the player equipped weapons from the selected player class
    /// </summary>
    public void SetupLoadout()
    {
        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0);
            if (child.GetComponent<bl_WeaponContainer>() != null)
            {
                Debug.LogWarning("This player prefab has the weapon container inside, delete it from the player prefab to avoid including the weapons along with the player prefab.");
                Destroy(child.gameObject);
            }
        }

#if CLASS_CUSTOMIZER
        bl_ClassManager.Instance.SetUpClasses(this);
#else
        //when player instance select player class select in bl_RoomMenu
        bl_PlayerClassLoadout pcl = null;
        var currentClass = PlayerClass.Assault.GetSavePlayerClass();
        switch (currentClass)
        {
            case PlayerClass.Assault:
                pcl = m_AssaultClass;
                break;
            case PlayerClass.Recon:
                pcl = m_ReconClass;
                break;
            case PlayerClass.Engineer:
                pcl = m_EngineerClass;
                break;
            case PlayerClass.Support:
                pcl = m_SupportClass;
                break;
        }

        if (pcl == null)
        {
            Debug.LogError($"Player Class Loadout has not been assigned for the class {currentClass}");
            return;
        }

        CurrentLoadout = pcl;

        // Assign the player class loadout weapons
        AddWeaponToSlot(0, GetGunOnListById(pcl.Primary), true);
        AddWeaponToSlot(1, GetGunOnListById(pcl.Secondary), true);
        AddWeaponToSlot(2, GetGunOnListById(pcl.Letal), true);
        AddWeaponToSlot(3, GetGunOnListById(pcl.Perks), true);

        // if you want to add more weapons you can do it here
        // e.g AddWeaponToSlot(4, GetGunOnListById(pcl.Custom), true);

#endif
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            if (PlayerEquip[i] == null) continue;
            PlayerEquip[i].Initialized();
        }
    }
    
    /// <summary>
    /// Setup the equipeed weapons from the player class and the game settings
    /// </summary>
    void SetupEquippedWeapons()
    {
        //Disable all weapons in children and take the first one from the loadout
        foreach (bl_Gun g in PlayerEquip)
        {
            if (g == null) continue;

            g.Setup(true);
        }
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns == null) continue;

            guns.gameObject.SetActive(false);
        }

        MFPSRoomInfo roomSettings = bl_RoomSettings.GetRoomInfo();
        // if the room has a weapon type only, then we will disable all weapons that are not the selected type
        if (roomSettings != null && roomSettings.WeaponOption != 0)
        {
            GunType onlyType = bl_GameData.Instance.allowedWeaponOnlyOptions[roomSettings.WeaponOption - 1];
            bool hasWeapon = false;
            for (int i = 0; i < PlayerEquip.Count; i++)
            {
                if (PlayerEquip[i].OriginalWeaponType == onlyType)
                {
                    hasWeapon = true;
                    if (i != 0)
                    {
                        PlayerEquip[0] = PlayerEquip[i];
                        PlayerEquip[i] = null;
                    }
                    continue;
                }

                PlayerEquip[i] = null;
            }

            if (!hasWeapon)
            {
                // try to set an available weapon from the inventory
                if (weaponContainer != null)
                {
                    var allWeapons = weaponContainer.GetAllWeaponsPrefabs(true);
                    for (int i = 0; i < allWeapons.Count; i++)
                    {
                        if (allWeapons[i].GetWeaponType() == onlyType && allWeapons[i].Info.Unlockability.IsUnlocked(allWeapons[i].GetGunID()))
                        {
                            Debug.Log($"Setup weapon {allWeapons[i].GetGunID()} for only: {onlyType}");
                            AddWeaponToSlot(0, GetWeapon(allWeapons[i].GetGunID()), true);
                            hasWeapon = true;
                            break;
                        }
                    }
                }
                else
                {
                    var allWeapons = AllGuns;
                    for (int i = 0; i < allWeapons.Count; i++)
                    {
                        if (allWeapons[i].GetWeaponType() == onlyType && allWeapons[i].Info.Unlockability.IsUnlocked(allWeapons[i].GetGunID()))
                        {
                            AddWeaponToSlot(0, allWeapons[i], true);
                            hasWeapon = true;
                            break;
                        }
                    }
                }
            }

            if (!hasWeapon)
            {
                Debug.LogWarning($"The room has a weapon type only, but the player class does not have a weapon of that type, the player would have no weapon.");
            }
        }

        if (bl_WeaponLoadoutUIBase.Instance != null) bl_WeaponLoadoutUIBase.Instance.SetLoadout(PlayerEquip);
        EquipPickUpMode = bl_GameData.CoreSettings.switchToPickupWeapon;

        var firstWeapon = PlayerEquip[currentWeaponIndex];
        ActiveWeaponInstance(firstWeapon);

        if (firstWeapon != null)
            bl_EventHandler.ChangeWeaponEvent(firstWeapon.GunID);
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
        if (!CanSwich || bl_GameData.isChatting)
            return;

        if (GetGameMode == GameMode.GR) return;

        // switch to a weapon by pressing the number key
        for (int i = 0; i < 4; i++)
        {
            if (bl_GameInput.WeaponSlot(i))
            {
                ChangeCurrentWeaponTo(i);
            }
        }

        //fast fire knife
        if (bl_GameInput.QuickMelee() && PlayerEquip[meleeSlot] != null && PlayerEquip[meleeSlot].m_AllowQuickFire && currentWeaponIndex != meleeSlot && !isFastFire)
        {
            DoFastKnifeShot(meleeSlot);
        }

        //fast throw grenade
        if (bl_GameInput.QuickNade() && PlayerEquip[lethalSlot] != null && PlayerEquip[lethalSlot].AllowQuickFire() && currentWeaponIndex != lethalSlot && !isFastFire)
        {
            DoSingleGrenadeThrow(lethalSlot);
        }

        if (bl_GameInput.BlockWeaponScroll || !HasEquippedSlots())
            return;

        //change gun with Scroll mouse
        if (bl_Input.isButtonDown("NextWeapon"))
        {
            SwitchNext();
        }
        if (bl_Input.isButtonDown("PreviousWeapon"))
        {
            SwitchPrevious();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchNext()
    {
        if (!HasEquippedSlots())
            return 0;
        if (GetGameMode == GameMode.GR) return 0;

        int next = (currentWeaponIndex + 1) % PlayerEquip.Count;
        if (PlayerEquip[next] == null) return 0;

        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchPrevious()
    {
        if (!HasEquippedSlots())
            return 0;
        if (GetGameMode == GameMode.GR) return 0;

        int next = currentWeaponIndex != 0 ? (currentWeaponIndex - 1) % PlayerEquip.Count : PlayerEquip.Count - 1;
        if (PlayerEquip[next] == null) return 0;
        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoFastKnifeShot(int slotId)
    {
        var equippedKnife = GetEquippedWeapon(slotId);
        if (equippedKnife == null || equippedKnife.Info.Type != GunType.Melee) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = slotId;
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedKnife.gameObject.SetActive(true);
        equippedKnife.QuickMelee(OnReturnWeapon);
        PlayerReferences.playerAnimations.CustomCommand(PlayerAnimationCommands.QuickMelee, equippedKnife.GunID.ToString());
        CanSwich = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoSingleGrenadeThrow(int slotIndex)
    {
        var equippedGrenade = PlayerEquip[slotIndex];
        if (equippedGrenade == null || equippedGrenade.Info.Type != GunType.Grenade) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = slotIndex;
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedGrenade.gameObject.SetActive(true);
        equippedGrenade.QuickGrenadeThrow(OnReturnWeapon);
        PlayerReferences.playerAnimations.CustomCommand(PlayerAnimationCommands.QuickGrenade, equippedGrenade.GunID.ToString());
        CanSwich = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnReturnWeapon()
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = PreviousGun;
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(true);
        CanSwich = true;
        isFastFire = false;
        bl_WeaponLoadoutUIBase.Instance?.SetEquippedSlot(currentWeaponIndex);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveWeaponInstance(bl_Gun gun, bool forced = false)
    {
        if (!CanSwich && !forced) return;
        if (gun == null) return;
        gun.gameObject.SetActive(true);
        CanSwich = true;
    }

    /// <summary>
    /// When this is called, all the equipped weapons are disabled and the
    /// player can't switch weapons
    /// </summary>
public void BlockAllWeapons()
    {

        foreach (bl_Gun g in PlayerEquip)
        {
            if (PlayerEquip == null) continue;
            if (g == null) continue;
            g.gameObject.SetActive(false); 
        }
        bl_CrosshairBase.Instance.Show(false);
        bl_CrosshairBase.Instance.Block = true;
        CanSwich = false;
        bl_GameManager.Instance.LocalPlayerReferences.playerNetwork.SetWeaponBlocked(1);
    }

    /// <summary>
    /// Call this after called <see cref="BlockAllWeapons"/> to active the weapons again
    /// </summary>
    public void ReleaseWeapons(bool takeFirst)
    {
        CanSwich = true;
        bl_CrosshairBase.Instance.Block = false;
        bl_CrosshairBase.Instance.Show(true);

        if (!HasAWeaponActive())
        {
            if (takeFirst) ActiveWeaponInstance(GetEquippedWeapon(0));
            else ActiveWeaponInstance(PlayerEquip[currentWeaponIndex]);
        }

        PlayerReferences.playerNetwork.SetWeaponBlocked(0);
    }

    /// <summary>
    /// Unequipped all the player weapons leaving him with nothing to attack.
    /// After this is called, you have to manually equip a weapon to the player.
    /// </summary>
    public void UnEquipEverything(bool hideCrosshair = true)
    {
        foreach (bl_Gun g in PlayerEquip) { if (g != null) g.gameObject.SetActive(false); }
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            PlayerEquip[i] = null;
        }
        if (hideCrosshair)
        {
            bl_CrosshairBase.Instance.Show(false);
            bl_CrosshairBase.Instance.Block = true;
        }
        bl_EventHandler.ChangeWeaponEvent(-1);
    }

    /// <summary>
    /// For test porpuses mostly
    /// After call this, all the weapons would have infinite ammo
    /// This have to be called after the player has been spawned
    /// </summary>
    public void SetInfinityAmmoToAllEquippeds(bool infinity)
    {
        AllGuns.ForEach((x) =>
        {
            if (x != null) x.SetInifinityAmmo(infinity);
        });
    }

    /// <summary>
    /// Change the current equipped weapon to the one with the given index from the <see cref="AllGuns"/> list"/>
    /// </summary>
    /// <param name="AllWeaponsIndex"></param>
    public void ChangeTo(int AllWeaponsIndex)
    {
        StartCoroutine(ChangeGun(currentWeaponIndex, AllGuns[AllWeaponsIndex].gameObject, currentWeaponIndex));
        PlayerEquip[currentWeaponIndex] = AllGuns[AllWeaponsIndex];
    }

    /// <summary>
    /// Change to a weapon without delay/animation
    /// </summary>
    public void ChangeToInstant(int AllWeaponsIndex)
    {
        ChangeToInstant(AllGuns[AllWeaponsIndex]);
    }

    /// <summary>
    /// Change to a weapon without delay/animation
    /// </summary>
    public void ChangeToInstant(bl_Gun weapon)
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        weapon.gameObject.SetActive(true);
        PlayerEquip[currentWeaponIndex] = weapon;
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
    }

    /// <summary>
    /// Change the current equipped weapon to the one the one in the provided weapon slot
    /// </summary>
    /// <param name="nextSlotID">index of the weapon in the <see cref="PlayerEquip"/> list</param>
    public void ChangeCurrentWeaponTo(int nextSlotID)
    {
        if (currentWeaponIndex == nextSlotID || PlayerEquip[nextSlotID] == null) return;
        if (!CanSwich || bl_GameData.isChatting)
            return;

        var next = PlayerEquip[nextSlotID];
        if (next == null || !next.CanBeTaken()) return;//if the weapons can't be taken when is empty (of ammo)

        PreviousGun = currentWeaponIndex;
        StartCoroutine(ChangeGun(currentWeaponIndex, next.gameObject, nextSlotID));
        currentWeaponIndex = nextSlotID;
    }

    /// <summary>
    /// Coroutine to Change of Gun
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangeGun(int IDfrom, GameObject t_next, int newID)
    {
        CanSwich = false;
        bl_WeaponLoadoutUIBase.Instance?.SetEquippedSlot(newID);
        float hideTime = PlayerEquip[IDfrom].DisableWeapon();
        //instantly disable the current weapon and active the next weapon
        if (changeWeaponStyle == ChangeWeaponStyle.CounterStrike)
        {
            PlayerEquip[IDfrom].gameObject.SetActive(false);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideAndDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait a fixed delay before active the next weapon
            yield return new WaitForSeconds(SwichTime);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideCompletelyAndThenDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait until the current weapon hide animation complete before active the next weapon
            yield return new WaitForSeconds(hideTime);
        }
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns == null) continue;
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        ActiveWeaponInstance(PlayerEquip[newID], true);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[newID].GunID);
    }

    /// <summary>
    /// This function is called when the local player pick up a weapon in the map
    /// </summary>
    public void PickUpGun(GunPickUpData e)
    {
        if (bl_GunPickUpManagerBase.Instance == null)
        {
            Debug.LogWarning("Need a 'Pick Up Manager' in the scene!");
            return;
        }
        //find the pick up weapon in the FP weapon list of this player prefab
        bl_Gun pickedLocalGun = GetWeapon(e.ID);
        if (pickedLocalGun == null)
        {
            Debug.LogWarning($"The weapon {e.ID} is not integrated in this player prefab.");
            return;
        }
        //If not already equipped
        if (!PlayerEquip.Exists(x => x != null && x.GunID == e.ID))
        {
            bl_GunInfo gunInfo = bl_GameData.Instance.GetWeapon(e.ID);
            //first we need to make sure that there's not an empty slot in the loadout
            for (int i = 0; i < PlayerEquip.Count; i++)
            {
                //if there's an empty slot
                if (PlayerEquip[i] == null)
                {
                    //check if the this weapon can by equipped in this slot
                    if (bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode.IsEnumFlagPresent(AutoChangeOnPickup.OnlyOnEmptySlots | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(null, pickedLocalGun, null, i));
                            currentWeaponIndex = i;
                        }
                        return;
                    }
                }
            }

            //if get there means that there's not empty slots so we have to replace the current weapon
            //so first let's check if the pick up weapon can replace the weapon in the current slot.

            bool isEmpty = PlayerEquip[currentWeaponIndex] == null;
            int actualID = isEmpty ? -1 : PlayerEquip[currentWeaponIndex].GunID;

            //Get the required data from the weapon that we are going to replace
            int[] info = new int[2];
            int clips = isEmpty ? 0 : (int)PlayerEquip[currentWeaponIndex].RemainingClips;
            info[0] = clips;
            info[1] = isEmpty ? 0 : PlayerEquip[currentWeaponIndex].bulletsLeft;

            bl_Gun oldGun;
            //if the pick up weapon can't replace the current slot
            if (!bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, currentWeaponIndex))
            {
                //let's find out the a compatible slot for it and replace the weapon there
                for (int i = 0; i < 4; i++)
                {
                    //when find the slot for this pick up weapon
                    if (bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        oldGun = PlayerEquip[i];
                        info[0] = (int)oldGun.RemainingClips;
                        info[1] = oldGun.bulletsLeft;

                        //change the weapon on that slot with the pick up one
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode == (AutoChangeOnPickup.OnlyOnReplacements | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, i));
                            currentWeaponIndex = i;
                        }
                        else
                        {
                            bl_GunPickUpManagerBase.Instance.ThrowGun(new bl_GunPickUpManagerBase.ThrowData()
                            {
                                GunID = oldGun.GunID,
                                Origin = GetThrowPosition(),
                                Direction = GetThrowDirection(),
                                Data = info,
                                AutoDestroy = false,
                                WeaponType = oldGun.OriginalWeaponType
                            });
                        }
                        return;
                    }
                }
            }

            oldGun = PlayerEquip[currentWeaponIndex];
            //Replace the current equipped weapon
            SetUpPickUpWeapon(e, pickedLocalGun, currentWeaponIndex);
            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, currentWeaponIndex));
        }
        else//if the weapon is already equipped
        {
            // pick up it as ammo only
            foreach (bl_Gun g in PlayerEquip)
            {
                if (g != null && g.GunID == e.ID)
                {
                    g.OnPickUpAmmo(e.Bullets * e.Clips, 1, e.ID);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Sets up a picked up weapon for the player.
    /// </summary>
    /// <param name="data">The data of the picked up weapon.</param>
    /// <param name="gun">The gun to be set up.</param>
    /// <param name="slotID">The slot ID where the gun will be placed.</param>
    void SetUpPickUpWeapon(GunPickUpData data, bl_Gun gun, int slotID)
    {
        PlayerEquip[slotID] = gun;
        gun.Setup(true);

        new MFPSLocalNotification($"Picked up {gun.Info.Name}");

        if (gun.Info.Type == GunType.Grenade)
        {
            gun.bulletsLeft = data.Bullets < 5 ? data.Bullets : 1;
            gun.RemainingClips = data.Clips < 5 ? data.Clips : 0;
        }
        else
        {
            gun.bulletsLeft = data.Bullets;
            gun.RemainingClips = data.Clips;
        }
        bl_WeaponLoadoutUIBase.Instance?.ReplaceSlot(slotID, gun);
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator SwitchToPickUpGun(bl_Gun currentGun, bl_Gun nextGun, int[] info, int newSlotID)
    {
        HeadAnimator?.Play("TakeGun", 0, 0);
        currentGun?.DisableWeapon();

        if (currentWeaponIndex != newSlotID) bl_WeaponLoadoutUIBase.Instance?.SetEquippedSlot(newSlotID);

        yield return new WaitForSeconds(PickUpTime);

        AllGuns.ForEach(x => { if (x != null) { x.gameObject.SetActive(false); } });
        ActiveWeaponInstance(nextGun, true);

        if (currentGun != null)
        {
            bl_GunPickUpManagerBase.Instance.ThrowGun(new bl_GunPickUpManagerBase.ThrowData()
            {
                GunID = currentGun.GunID,
                Origin = GetThrowPosition(),
                Direction = GetThrowDirection(),
                Data = info,
                AutoDestroy = false,
                WeaponType = currentGun.OriginalWeaponType
            });
        }

        bl_EventHandler.ChangeWeaponEvent(nextGun.GunID);
        CanSwich = true;
    }

    /// <summary>
    /// Throws the current equipped weapon.
    /// </summary>
    /// <param name="AutoDestroy">Whether the thrown weapon should be automatically destroyed.</param>
    /// <param name="throwPosition">The position to throw the weapon from.</param>
    public void ThrowCurrent(bool AutoDestroy, Vector3 throwPosition)
    {
        if (PlayerEquip[currentWeaponIndex] == null)
            return;

        int actualID = PlayerEquip[currentWeaponIndex].GunID;
        int[] info = new int[2];
        int clips = (bl_GameData.CoreSettings.AmmoType == AmmunitionType.Bullets) ?
            (int)PlayerEquip[currentWeaponIndex].RemainingClips / PlayerEquip[currentWeaponIndex].bulletsPerClip : (int)PlayerEquip[currentWeaponIndex].RemainingClips;

        info[0] = clips;
        info[1] = PlayerEquip[currentWeaponIndex].bulletsLeft;

        bl_GunPickUpManagerBase.Instance.ThrowGun(new bl_GunPickUpManagerBase.ThrowData()
        {
            GunID = actualID,
            Origin = throwPosition,
            Direction = transform.forward,
            Data = info,
            AutoDestroy = AutoDestroy,
            WeaponType = PlayerEquip[currentWeaponIndex].OriginalWeaponType
        });
    }

    /// <summary>
    /// Return the weapon equipped in the giving slot
    /// </summary>
    /// <param name="slotID">The index of the weapon in the <see cref="PlayerEquip"/> list.</param>
    /// <returns></returns>
    public bl_Gun GetEquippedWeapon(int slotID)
    {
        if (!HasEquippedSlots())
            return null;
        if (PlayerEquip.Count - 1 < slotID)
        {
            Debug.LogWarning($"The slot {slotID} is not assigned in the player prefab '{bl_GameManager.Instance.LastInstancedPlayerPrefab.name}'");
            return null;
        }

        return PlayerEquip[slotID];
    }

    /// <summary>
    /// Retrieves a gun from the list of all guns based on its ID.
    /// If the gun is not found in the list, it checks the weapon container for the gun.
    /// If the gun is still not found, it logs an error message.
    /// </summary>
    /// <param name="id">The ID of the gun to retrieve.</param>
    /// <returns>The gun with the specified ID, or null if it is not found.</returns>
    public bl_Gun GetGunOnListById(int id)
    {
        bl_Gun gun = null;
        if (AllGuns.Exists(x => x != null && x.GunID == id))
        {
            gun = AllGuns.Find(x => x.GunID == id);
        }
        else
        {
            var weapon = GetWeaponFromContainer(id, transform);
            if (weapon != null)
            {
                return weapon;
            }

            Debug.LogError($"The FPWeapon: {id} has not been integrated in this player prefab '{bl_GameManager.Instance.LastInstancedPlayerPrefab.name}' neither exist in the weapon container");
        }
        return gun;
    }

    /// <summary>
    /// Get a weapon instance by the GunID
    /// This function will try to find the weapon in the player instance first
    /// if the weapon is not found, it will try to find it in the weapon container and instance it if it is found there.
    /// </summary>
    /// <param name="GunID"></param>
    /// <returns></returns>
    public bl_Gun GetWeapon(int GunID)
    {
        int index = AllGuns.FindIndex(x => x != null && x.GunID == GunID);
        if (index != -1)
        {
            return AllGuns[index];
        }

        var weapon = GetWeaponFromContainer(GunID, transform);
        return weapon != null ? weapon : null;
    }

    /// <summary>
    /// Get and Instantiate a FP weapon from the weapon container prefab
    /// This should be called only after check that the weapon is not already instanced
    /// </summary>
    /// <param name="gunID"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public bl_Gun GetWeaponFromContainer(int gunID, Transform parent)
    {
        if (weaponContainer == null) return null;
        if (AllGuns.Exists(x => x != null && x.GunID == gunID))
        {
            Debug.LogWarning($"You are instantiating the weapon {gunID} from the weapon container but that weapon was already instanced.");
        }

        bl_WeaponBase weapon = weaponContainer.GetWeapon(gunID, transform);
        if (weapon != null)
        {
            var castWeapon = weapon as bl_Gun;
            AllGuns.Add(castWeapon);
            return castWeapon;
        }
        return null;
    }

    /// <summary>
    /// Called when the game settings changed in runtime
    /// </summary>
    public void OnGameSettingsChanged()
    {
        if (bl_MFPS.Settings == null) return;

        float fov = (float)bl_MFPS.Settings.GetSettingOf("Weapon FOV");
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            item.SetDefaultWeaponCameraFOV(fov);
        }
    }

    /// <summary>
    /// Add a weapon to the player quipped weapons list
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="weapon"></param>
    /// <param name="addSlotIfNotExist"></param>
    public void AddWeaponToSlot(int slot, bl_Gun weapon, bool addSlotIfNotExist = false)
    {
        PlayerEquip ??= new List<bl_Gun>();

        if (PlayerEquip.Count - 1 < slot)
        {
            if (!addSlotIfNotExist)
            {
                Debug.LogWarning($"Weapon slot {slot} has not been preassigned.");
                return;
            }
            else
            {
                while (PlayerEquip.Count - 1 < slot)
                {
                    PlayerEquip.Add(null);
                }
            }
        }

        PlayerEquip[slot] = weapon;
    }
    void PlayDrinkAudio()
    {
       fireAudioSource.clip = DrinkAudioClip;
       fireAudioSource.Play();
    }

  public void Drinking()
  {
      if (Drink != null)
      {
          Drink.SetActive(true);
          PlayDrinkAudio();
          animatorDrink.Play("Drink");
      }

  }
  public void StopDrink()
  {
      if (Drink != null)
      {
          if (useHideAnimationDrink)
          {
              animatorDrink.Play("HideDrink");
          }
          Drink.SetActive(false);
          fireAudioSource.Stop();
      }
  }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetThrowPosition()
    {
#if MFPSTPV
        if (bl_CameraViewSettings.IsThirdPerson())
        {
            return transform.root.TransformPoint(new Vector3(0, 1, 1) * 0.55f);
        }
#endif
        return PlayerReferences.PlayerCameraTransform.TransformPoint(Vector3.forward * 0.55f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetThrowDirection()
    {
#if MFPSTPV
        if (bl_CameraViewSettings.IsThirdPerson())
        {
            return transform.root.forward;
        }
#endif
        return PlayerReferences.PlayerCameraTransform.forward;
    }

    /// <summary>
    /// 
    /// </summary>
    public void HeadAnimation(int state, float speed)
    {
        if (HeadAnimator == null)
            return;
#if MFPSTPV
        if (bl_CameraViewSettings.IsThirdPerson()) return;
#endif
        switch (state)
        {
            case 0:
                HeadAnimator.SetInteger("Reload", 0);
                break;
            case 1:
                HeadAnimator.SetInteger("Reload", 1);
                break;
            case 2:
                HeadAnimator.SetInteger("Reload", 2);
                break;
            case 3:
                HeadAnimator.Play("Insert", 0, 0);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AudioSource GetFireAudioSource()
    {
        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.loop = false;
            fireAudioSource.playOnAwake = false;
            fireAudioSource.AssignMixerGroup("FP Weapon");
        }

        return fireAudioSource;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Gun GetCurrentWeapon()
    {
        return CurrentGun;
    }

    /// <summary>
    /// The GunID of the current equipped weapon
    /// -1 means that the player doesn't have a weapon equipped yet
    /// </summary>
    public int GetCurrentGunID
    {
        get
        {
            return GetCurrentWeapon() == null ? -1 : GetCurrentWeapon().GunID;
        }
    }

    void OnMatchStart() { IsGameStarted = true; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasAWeaponActive()
    {
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            if (item.gameObject.activeSelf) return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasEquippedSlots()
    {
        return PlayerEquip != null && PlayerEquip.Count > 0;
    }

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }

    [System.Serializable]
    public enum ChangeWeaponStyle
    {
        HideAndDraw,
        CounterStrike,
        HideCompletelyAndThenDraw,
    }

    [System.Serializable, System.Flags]
    public enum AutoChangeOnPickup
    {
        Always = 0,
        OnlyOnEmptySlots,
        OnlyOnReplacements,
        Never,
    }
}
using MFPS.Audio;
using MFPS.Core.Motion;
using MFPSEditor;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_PlayerHealthManager : bl_PlayerHealthManagerBase
{
    #region Public members
    [Header("Settings")]
    [Range(0, 100)] public int health = 100;
    [Range(1, 100)] public int maxHealth = 100;
    [Range(1, 10)] public float StartRegenerateIn = 4f;
    [Range(1, 5)] public float RegenerationSpeed = 3f;
    [Range(10, 100)] public int RegenerateUpTo = 100;

    [Header("Shake")]
    [ScriptableDrawer] public ShakerPresent damageShakerPresent;

    [Header("Effects")]
    public AudioClip[] HitsSound;
    [SerializeField] private AudioClip[] InjuredSounds = null;

    private bool m_HealthRegeneration = false;
    public bool HealthRegeneration { get => m_HealthRegeneration; set => m_HealthRegeneration = value; }
    #endregion

    #region Private members
    const string FallMethod = "FallDown";
    private bool isDead = false;
    private string lastDamageGiverActor;
    private readonly int ScorePerKill, ScorePerHeatShot;
    private float TimeToRegenerate = 4;
    private bl_GunManager GunManager;
    private bool isSuscribed = false;
    private int protecTime = 0;
    private RepetingDamageInfo repetingDamageInfo;
    private bool showIndicator = false;
    private float nextHealthSend = 0;
    private bl_PlayerReferences playerReferences;
#if !ACTK_IS_HERE
    private int currentHealth = 0;
#else
    private ObscuredInt currentHealth = 0;
#endif
    #endregion

    #region Unity Callbacks
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected) return;

        base.Awake();
        TryGetComponent(out playerReferences);
        GunManager = playerReferences.gunManager;
        m_HealthRegeneration = bl_GameData.CoreSettings.HealthRegeneration;
        protecTime = bl_GameData.CoreSettings.SpawnProtectedTime;
        showIndicator = bl_GameData.CoreSettings.showDamageIndicator;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!IsConnected)
            return;

        SetCurrentHealth(health);
        if (IsMine)
        {
            bl_MFPS.LocalPlayer.IsAlive = true;
            gameObject.name = bl_PhotonNetwork.NickName;
        }
        if (protecTime > 0) { InvokeRepeating(nameof(OnProtectCount), 1, 1); }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (this.IsMine)
        {
            bl_MFPS.LocalPlayer.ViewID = this.photonView.ViewID;
            bl_EventHandler.onPickUpHealth += this.OnPickUp;
            bl_EventHandler.onRoundEnd += this.OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
            isSuscribed = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (isSuscribed)
        {
            bl_EventHandler.onPickUpHealth -= this.OnPickUp;
            bl_EventHandler.onRoundEnd -= this.OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (IsMine)
        {
            RegenerateHealth();
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public override void DoDamage(DamageData damageData)
    {
        GetDamage(damageData);
    }

    /// <summary>
    /// Call this to do damage to this player
    /// </summary>
    public void GetDamage(DamageData e)
    {
        bool canDamage = true;
        if (!DamageEnabled)
        {
            //Fix: bots can't damage Master Client teammates.
            if (e.MFPSActor != null && (e.MFPSActor.Team != playerReferences.PlayerTeam || bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire))
            {
                canDamage = true;
            }
            else canDamage = false;
        }

        if (!canDamage || IsProtectionEnable || (bl_GameManager.Instance != null && bl_GameManager.Instance.GameMatchState == MatchState.Waiting))
        {
            if (!IsMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            return;
        }

        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, e);
    }

    /// <summary>
    /// This function is called on all clients when this player receive damage
    /// </summary>
    [PunRPC]
    void SyncDamage(DamageData damageData, PhotonMessageInfo messageInfo)
    {
        if (isDead || IsProtectionEnable)
            return;

        Player sender = messageInfo.Sender;

        if (DamageEnabled)
        {
            if (IsMine)
            {
                bl_EventHandler.DoPlayerCameraShake(damageShakerPresent, "damage");
                if (showIndicator) bl_DamageIndicatorBase.Instance?.SetHit(damageData.OriginPosition);
                TimeToRegenerate = StartRegenerateIn;
                bl_EventHandler.DispatchLocalPlayerReceiveDamage(damageData.Damage);
            }
            else
            {
                bool isLethal = (currentHealth - damageData.Damage) < 1;
                var hitData = new MFPSHitData()
                {
                    HitTransform = transform,
                    HitPosition = transform.position,
                    Damage = damageData.Damage,
                    HitName = gameObject.name,
                    PlayerAutorName = damageData.MFPSActor != null ? damageData.MFPSActor.Name : string.Empty,
                    WasLethal = isLethal,
                };

                // When the damage is given by the local player to another player
                if (sender != null && sender.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber && damageData.Cause != DamageCause.Bot)
                {
                    if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnHit(isLethal);
                    bl_AudioController.PlayClip(isLethal ? "head-hit" : "body-hit");
                    bl_EventHandler.DispatchLocalPlayerHitEnemy(hitData);
                }
                // if the hit was caused by a bot
                else if (!damageData.IsActorARealPlayer())
                {
                    bl_EventHandler.Bots.onBotHitPlayer(hitData);
                }
            }

            if (damageData.Cause != DamageCause.FallDamage && damageData.Cause != DamageCause.Fire)
            {
                if (HitsSound.Length > 0)//Audio effect of hit
                {
                    AudioSource.PlayClipAtPoint(HitsSound[Random.Range(0, HitsSound.Length)], transform.position, 1.0f);
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(InjuredSounds[Random.Range(0, InjuredSounds.Length)], transform.position, 1.0f);
            }

            lastDamageGiverActor = damageData.From;
        }

        if (currentHealth > 0)
        {
            SetCurrentHealth(currentHealth - damageData.Damage);
            if (!IsMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
        }

        if (currentHealth < 1)
        {
            SetCurrentHealth(0);

            PlayerDie(true, lastDamageGiverActor, damageData.IsHeadShot, damageData.Cause, damageData.GunID, damageData.OriginPosition, sender);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    void PlayerDie(bool syncToAll, string killer, bool headShot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        isDead = true;
        if (syncToAll)
        {
            if (bl_PhotonNetwork.IsMasterClient) photonView.RPC(nameof(PlayerDie), RpcTarget.All, false, lastDamageGiverActor, headShot, cause, gunID, hitPos, sender);
            return;
        }

        if (IsMine)
        {
            bl_MFPS.LocalPlayer.IsAlive = false;
            bl_EventHandler.DispatchPlayerLocalDeathEvent();
        }

        Die(killer, headShot, cause, gunID, hitPos, sender);
    }

    /// <summary>
    /// This is called when this player die in match
    /// This function determine the cause of death and update the require stats
    /// </summary>
	void Die(string killer, bool isHeadshot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        // Debug.Log($"{gameObject.name} die cause {cause.ToString()} from {killer} and GunID {gunID}");
        isDead = true;
        transform.parent = null;
        playerReferences.characterController.enabled = false;
        if (bl_GameManager.Instance != null) bl_GameManager.Instance.GetMFPSPlayer(gameObject.name).isAlive = false;
        var gameData = bl_GameData.Instance;
        bl_GunInfo gunInfo = gameData.GetWeapon(gunID);
        bool isExplosion = gunInfo.Type == GunType.Grenade || gunInfo.Type == GunType.Launcher;
        playerReferences.onDie?.Invoke();

        if (!IsMine)
        {
            playerReferences.playerRagdoll.Ragdolled(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                IsFromExplosion = isExplosion,
                AutoDestroy = true
            });// convert into ragdoll the remote player
        }
        else
        {
            //Make the remote players drop their weapon
            Transform ngr = (gameData.coreSettings.DropGunOnDeath == false) ? null : playerReferences.RemoteWeapons != null ? playerReferences.RemoteWeapons.transform : null;
            if (ngr != null) { ngr.gameObject.SetActive(false); }

            playerReferences.playerRagdoll.SetLocalRagdoll(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                Velocity = playerReferences.characterController.velocity,
                IsFromExplosion = isExplosion,
                AutoDestroy = true,
                RightHandChild = ngr
            });
        }

        //disable all other player prefabs child's
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        string weapon = cause.ToString();
        if (cause == DamageCause.Player || cause == DamageCause.Bot || cause == DamageCause.Explosion)
        {
            weapon = gunInfo.Name;
        }

        var mplayer = new MFPSPlayer(photonView, true, false);
        var eDeathData = new bl_EventHandler.PlayerDeathData()
        {
            Player = mplayer,
            KillerName = killer
        };
        bl_EventHandler.onPlayerDeath?.Invoke(eDeathData);

        if (!IsMine)// when player is not ours
        {
            //if the local player was the one who kill this player
            if (lastDamageGiverActor == LocalName)
            {
                AddKill(isHeadshot, weapon, gunID, cause, hitPos);
            }

            // Show an icon on the map where the player died (only if is a teammate)
            if (gameData.coreSettings.showDeathIcons && playerReferences.IsTeamMateOfLocalPlayer())
            {
                bl_ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
            }

            bl_EventHandler.DispatchRemotePlayerDeath(eDeathData);
        }
        else //when is the local player who have been eliminated
        {

            //Set to respawn again
            if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
            {
                if (bl_GameManager.Instance != null) bl_GameManager.Instance.RespawnLocalPlayerAfter();
            }

            if (cause == DamageCause.Bot)
            {
                // increase the deaths count for the local player
                if (gameData.coreSettings.howConsiderBotsEliminations == MFPS.Runtime.AI.BotKillConsideration.SameAsRealPlayers)
                    bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }
            else
            {
                // increase the deaths count for the local player
                bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }

            //Show the kill camera
            bl_KillCamBase.Instance.SetTarget(new bl_KillCamBase.KillCamInfo()
            {
                RealPlayer = sender,
                TargetName = killer,
                GunID = gunID,
                Target = transform,
                FallbackTarget = playerReferences.playerRagdoll.transform
            }).SetActive(true);

            // if the local player was eliminated by himself
            if (killer == LocalName)
            {
                if (cause == DamageCause.FallDamage)
                {
                    bl_KillFeedBase.Instance.SendTeamHighlightMessage(LocalName, bl_GameTexts.DeathByFall.Localized(20), bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());

                }
                else
                {
                    bl_KillFeedBase.Instance.SendTeamHighlightMessage(LocalName, bl_GameTexts.CommittedSuicide.Localized(19), bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                }
            }

            if (gameData.coreSettings.DropGunOnDeath && playerReferences.RemoteWeapons != null)
            {
                GunManager.ThrowCurrent(true, playerReferences.RemoteWeapons.transform.position + transform.forward);
            }

            if (cause == DamageCause.Bot)
            {
                Team botTeam = Team.All;
                if (!isOneTeamMode) botTeam = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam().OppsositeTeam();
                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = gameObject.name,
                    Team = botTeam
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", isHeadshot);

                bl_KillFeedBase.Instance.SendKillMessageEvent(feed);
                //the local player will update the bot stats instead of the Master in this case.
                bl_AIMananger.SetBotKill(killer);
            }
            StartCoroutine(DestroyThis());
        }
    }

    /// <summary>
    /// when we get a new kill, synchronize and add points to the player
    /// </summary>
    public void AddKill(bool isHeadshot, string m_weapon, int gunID, DamageCause cause, Vector3 direction)
    {
        // if is an special weapon
        if (gunID >= 300)
        {
            gunID -= 299;
            gunID = -gunID;
        }

        //send kill feed kill message
        var feed = new bl_KillFeedBase.FeedData()
        {
            LeftText = LocalName,
            RightText = gameObject.name,
            Team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()
        };
        feed.AddData("gunid", gunID);
        feed.AddData("headshot", isHeadshot);

        bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

        // if this was a friendly fire kill, don't add the score
        if (bl_MFPS.LocalPlayer.Team == playerReferences.PlayerTeam) return;

        //Add a new kill and update the player information
        bl_PhotonNetwork.LocalPlayer.PostKill(1);

        // calculate the score gained for this kill
        int score = (isHeadshot) ? bl_GameData.ScoreSettings.ScorePerKill + bl_GameData.ScoreSettings.ScorePerHeadShot : bl_GameData.ScoreSettings.ScorePerKill;

        //show an local notification for the kill
        var localKillInfo = new KillInfo
        {
            Killer = lastDamageGiverActor,
            Killed = gameObject.name,
            byHeadShot = isHeadshot,
            KillMethod = m_weapon,
            GunID = gunID,
            Cause = cause,
            FromPosition = direction,
            ToPosition = transform.position
        };
        bl_EventHandler.DispatchLocalKillEvent(localKillInfo);

        var elimatedTeam = photonView.Owner.GetPlayerTeam();
        if (isOneTeamMode)
        {
            // add the score to the player total gained score in this match
            bl_PhotonNetwork.LocalPlayer.PostScore(score);
        }
        else if (elimatedTeam != Team.All && elimatedTeam != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
        {
            // add the score to the player total gained score in this match
            bl_PhotonNetwork.LocalPlayer.PostScore(score);
        }
    }

    /// <summary>
    /// Do constant damage to the player in a loop until cancel.
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// Apply damage from a custom loop
    /// </summary>
    void MakeDamageRepeting()
    {
        if (repetingDamageInfo == null)
        {
            CancelRepetingDamage();
            return;
        }

        var damageinfo = repetingDamageInfo.DamageData;
        damageinfo ??= new DamageData
        {
            OriginPosition = Vector3.zero,
            Cause = DamageCause.Map
        };
        damageinfo.Damage = repetingDamageInfo.Damage;

        GetDamage(damageinfo);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// 
    /// </summary>
    void RegenerateHealth()
    {
        if (!m_HealthRegeneration || currentHealth < 1) return;
        if (currentHealth >= RegenerateUpTo) return;

        int projectedHealth = currentHealth;
        if (TimeToRegenerate <= 0)
        {
            projectedHealth = currentHealth;
            projectedHealth += 1;
        }
        else
        {
            TimeToRegenerate -= Time.deltaTime * 1.15f;
        }

        if (Time.time - nextHealthSend >= (1 / RegenerationSpeed))
        {
            nextHealthSend = Time.time;
            photonView.RPC(nameof(PickUpHealth), RpcTarget.All, projectedHealth);
        }
    }

    /// <summary>n
    /// Make the local player kill himself
    /// </summary>
    public override bool Suicide()
    {
        if (!IsMine || !bl_MFPS.LocalPlayer.IsAlive) return false;
        if (IsProtectionEnable) return false;

        DamageData e = new()
        {
            Damage = 500,
            From = base.LocalName,
            OriginPosition = transform.position,
            IsHeadShot = false
        };
        GetDamage(e);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnProtectCount()
    {
        protecTime--;
        if (IsMine)
        {
            if (bl_UIReferences.Instance != null) bl_UIReferences.Instance.OnSpawnCount(protecTime);
        }
        if (protecTime <= 0)
        {
            CancelInvoke(nameof(OnProtectCount));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(0.3f);
        DestroyEntity();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DestroyEntity()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    /// <summary>
    /// This event is called when player pick up a med kit
    /// </summary>
    /// <param name="amount"> amount for sum at current health</param>
    void OnPickUp(int amount)
    {
        SetHealth(amount);
    }

    /// <summary>
    /// Add health to this player and sync with all clients
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="replace"></param>
    public override void SetHealth(int amount, bool replace = false)
    {
        if (currentHealth < 1 || isDead) return;

        if (photonView.IsMine)
        {
            int newHealth = currentHealth + amount;
            if (replace) newHealth = amount;

            SetCurrentHealth(newHealth);
            if (currentHealth > maxHealth)
            {
                SetCurrentHealth(maxHealth);
            }
            photonView.RPC(nameof(PickUpHealth), RpcTarget.OthersBuffered, newHealth);
        }
    }

    [PunRPC]
    void RpcSyncHealth(int newHealth, PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == photonView.ViewID)
        {
            SetCurrentHealth((int)newHealth);
        }
    }

    /// <summary>
    /// Sync Health when pick up a med kit.
    /// </summary>
    [PunRPC]
    void PickUpHealth(int t_amount)
    {
        if (currentHealth < 1 || isDead) return;

        SetCurrentHealth((int)t_amount);
        if (currentHealth > maxHealth)
        {
            SetCurrentHealth(maxHealth);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newHealth"></param>
    private void SetCurrentHealth(int newHealth)
    {
        if (currentHealth != newHealth)
        {
            currentHealth = newHealth;
            if (IsMine) bl_EventHandler.Player.onLocalHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private bool IsProtectionEnable { get { return (protecTime > 0); } }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => currentHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => maxHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return isDead;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RpcSyncHealth), newPlayer, (int)currentHealth);
        }
    }

    /// <summary>
    /// When round is end 
    /// desactive some functions
    /// </summary>
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}
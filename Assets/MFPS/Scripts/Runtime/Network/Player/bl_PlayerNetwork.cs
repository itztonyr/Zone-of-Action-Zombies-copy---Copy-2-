using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_PlayerNetwork : bl_PlayerNetworkBase, IPunObservable
{
    #region Public members
    /// <summary>
    /// the player's team is not ours
    /// </summary>
    public Team RemoteTeam { get; set; }
    /// <summary>
    /// the object to which the player looked
    /// </summary>
    [FormerlySerializedAs("HeatTarget")]
    public Transform HeadTarget;
    /// <summary>
    /// smooth interpolation amount
    /// </summary>
    public float SmoothingDelay = 8f;
    /// <summary>
    /// list all remote weapons
    /// </summary>
    public List<bl_NetworkGun> NetworkGuns = new();
    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new();
    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new();
    public Material InvicibleMat;
    #endregion

    #region Private members
    private bl_NamePlateBase DrawName;
    private bool SendInfo = false;
    private bool isWeaponBlocked = false;
    private Transform m_Transform;
    private Vector3 networkHeadLookAt = Vector3.zero;// Head Look to
    private bool networkIsGrounded;
    private string RemotePlayerName = string.Empty;
    private Vector3 networkVelocity;
    PhotonTransformViewPositionControl m_PositionControl;
    PhotonTransformViewRotationControl m_RotationControl;
    bool m_ReceivedNetworkUpdate = false;
    private Vector3 headEuler = Vector3.zero;
    private int currentGunID = -1;
#if UMM
    private bl_MiniMapEntityBase MiniMapItem = null;
#endif
    #endregion

    #region Public Properties
    /// <summary>
    /// Is this player teammate of the local player?
    /// </summary>
    public bool IsFriend
    {
        get;
        set;
    }

    /// <summary>
    /// The current TPWeapon in the hands of this player
    /// </summary>
    public bl_NetworkGun CurrenGun
    {
        get;
        set;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        bl_EventHandler.Match.onSpectatorModeChanged += OnSpectatorModeChanged;
        if ((!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom) && bl_MFPS.IsMultiplayerScene) return;

        m_Transform = transform;
        m_PositionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        m_RotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        DrawName = GetComponent<bl_NamePlateBase>();
        NetworkGuns.ForEach(x =>
        {
            if (x != null) x.gameObject.SetActive(false);
        });
#if UMM
        MiniMapItem = this.GetComponent<bl_MiniMapEntityBase>();
        if (IsMine && MiniMapItem != null)
        {
            MiniMapItem.enabled = false;
        }
#endif

        SlowLoop();
        DrawName.SetActive(IsFriend);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        InvokeRepeating(nameof(SlowLoop), 0, 1);
    }

    /// <summary>
    /// serialization method of photon
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        m_PositionControl.OnPhotonSerializeView(m_Transform.localPosition, stream, info);
        m_RotationControl.OnPhotonSerializeView(m_Transform.localRotation, stream, info);

#if UNITY_EDITOR
        if (IsMine == false)
        {
            DoDrawEstimatedPositionError();
        }
#endif

        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(GetHeadLookPosition());
            stream.SendNext(FpControler.State);
            stream.SendNext(FpControler.isGrounded);
            stream.SendNext((byte)PlayerReferences.gunManager.GetCurrentGunID);//send as byte, max value is 255.
            stream.SendNext(FPState);
            stream.SendNext(FpControler.Velocity);
        }
        else
        {
            //Network player, receive data
            networkHeadLookAt = (Vector3)stream.ReceiveNext();
            NetworkBodyState = (PlayerState)stream.ReceiveNext();
            networkIsGrounded = (bool)stream.ReceiveNext();
            NetworkGunID = (int)((byte)stream.ReceiveNext());
            FPState = (PlayerFPState)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();

            m_ReceivedNetworkUpdate = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_EventHandler.Match.onSpectatorModeChanged -= OnSpectatorModeChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if ((!IsConnected || !bl_PhotonNetwork.InRoom) && bl_MFPS.IsMultiplayerScene) return;

        if (photonView != null && !IsMine)
        {
            OnRemotePlayer();
        }
        else
        {
            OnLocalPlayer();
        }
    }

    /// <summary>
    /// Function called each frame when the player is local
    /// </summary>
    void OnLocalPlayer()
    {
        //send the state of player local for remote animation
        PlayerReferences.playerAnimations.BodyState = FpControler.State;
        PlayerReferences.playerAnimations.IsGrounded = FpControler.isGrounded;
        PlayerReferences.playerAnimations.Velocity = FpControler.Velocity;
        PlayerReferences.playerAnimations.FPState = FPState;
    }

    /// <summary>
    /// Function called each frame when the player is remote
    /// </summary>
    void OnRemotePlayer()
    {
        if (m_Transform.parent == null)
        {
            UpdatePosition();
            UpdateRotation();
        }

        HeadTarget.LookAt(networkHeadLookAt);

        PlayerReferences.playerAnimations.BodyState = NetworkBodyState;//send the state of player local for remote animation
        PlayerReferences.playerAnimations.IsGrounded = networkIsGrounded;
        PlayerReferences.playerAnimations.Velocity = networkVelocity;
        PlayerReferences.playerAnimations.FPState = FPState;

        if (!isOneTeamMode)
        {
            //Determine if remote player is teamMate or enemy
            if (IsFriend)
                OnTeammatePlayer();
            else
                OnEnemyPlayer();
        }
        else
        {
            OnEnemyPlayer();
        }

        if (!isWeaponBlocked)
        {
            CurrentTPVGun();
            currentGunID = NetworkGunID;
        }
    }

    /// <summary>
    /// Function called each frame when this Remote player is an enemy
    /// </summary>
    void OnEnemyPlayer()
    {
        PlayerHealthManager.DamageEnabled = true;
#if UMM
        if (bl_MiniMapData.Instance.showEnemysWhenFire)
        {
#if KSA
            if (bl_KillStreakHandler.Instance != null && bl_KillStreakHandler.Instance.activeAUVs > 0) return;
#endif
            if (FPState == PlayerFPState.Firing || FPState == PlayerFPState.FireAiming)
            {
                MiniMapItem?.SetActiveIcon(true);
            }
            else
            {
                MiniMapItem?.SetActiveIcon(false);
            }
        }
#endif
    }

    /// <summary>
    /// Function called each frame when this Remote player is a teammate.
    /// </summary>
    void OnTeammatePlayer()
    {
        PlayerHealthManager.DamageEnabled = bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire;
        DrawName.SetActive(true);
        PlayerReferences.characterController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            PlayerReferences.playerRagdoll.IgnorePlayerCollider();
        }

#if UMM
        MiniMapItem?.SetActiveIcon(true);
#endif
    }

    /// <summary>
    /// This function control which TP Weapon should be showing on remote players.
    /// </summary>
    /// <param name="local">Should take the current gun from the local player or from the network data?</param>
    /// <param name="force">Double-check even if the equipped weapon has not changed.</param>
    public override void CurrentTPVGun(bool local = false, bool force = false)
    {
        if (PlayerReferences.gunManager == null)
            return;
        if (NetworkGunID == currentGunID && !force) return;

        bool found = false;

        //Get the current gun ID local and sync with remote
        for (int i = 0; i < NetworkGuns.Count; i++)
        {
            var tpWeapon = NetworkGuns[i];
            if (tpWeapon == null) continue;

            int currentID = (local) ? PlayerReferences.gunManager.GetCurrentWeapon().GunID : NetworkGunID;
            if (tpWeapon.GetWeaponID == currentID)
            {
                tpWeapon.gameObject.SetActive(true);
                if (!local)
                {
                    CurrenGun = tpWeapon;
                    CurrenGun.ActiveThisWeapon();
                }
                found = true;
            }
            else
            {
                if (tpWeapon != null)
                    tpWeapon.gameObject.SetActive(false);
            }
        }

        // if the tp weapon is not found in this player instance, then try to get it from the weapon container
        if (!found && PlayerReferences.RemoteWeapons != null)
        {
            var tpWeapon = PlayerReferences.RemoteWeapons.GetWeapon(NetworkGunID, PlayerReferences.PlayerAnimator.avatar);
            if (tpWeapon != null)
            {
                tpWeapon.gameObject.SetActive(true);
                CurrenGun = tpWeapon;
                CurrenGun.ActiveThisWeapon();
                NetworkGuns.Add(tpWeapon);
            }
        }
    }

    /// <summary>
    /// Show a TPWeapon temporaly without actually equip it.
    /// </summary>
    /// <param name="toGunId">the GunID of the weapon to switch</param>
    /// <param name="timeToShow">delay time in milliseconds to switch back to the equipped weapon.</param>
    public override async void TemporalySwitchTPWeapon(int toGunId, int timeToShow)
    {
        bl_NetworkGun toTPWeapon = GetTPWeapon(toGunId);
        bl_NetworkGun equippedWeapon = CurrenGun;
        if (toTPWeapon != null) toTPWeapon.gameObject.SetActive(true);
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(false);
        await Task.Delay(timeToShow);
        if (toTPWeapon != null) toTPWeapon.gameObject.SetActive(false);
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    void SlowLoop()
    {
        if (photonView == null || photonView.Owner == null) return;

        RemotePlayerName = photonView.Owner.NickName;
        RemoteTeam = photonView.Owner.GetPlayerTeam();
        gameObject.name = RemotePlayerName;
        if (DrawName != null) { DrawName.SetName(RemotePlayerName); }
        IsFriend = (RemoteTeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// Change the current TPWeapon on this remote player.
    /// </summary>
    public override void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun)
    {
        PlayerReferences.playerAnimations?.SetNetworkGun(weaponType, networkGun);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    /// <returns></returns>
    public bl_NetworkGun GetTPWeapon(int gunID)
    {
        int index = NetworkGuns.FindIndex(x =>
        {
            return x != null && x.GetWeaponID == gunID;
        });

        if (index != -1)
        {
            return NetworkGuns[index];
        }
        else if (PlayerReferences.RemoteWeapons != null)
        {
            var tpWeapon = PlayerReferences.RemoteWeapons.GetWeapon(gunID);
            if (tpWeapon != null)
            {
                NetworkGuns.Add(tpWeapon);
            }
            return tpWeapon;
        }

        return null;
    }

    /// <summary>
    /// Replicate a player command to all other clients
    /// </summary>
    /// <param name="commandId">Command Identifier</param>
    /// <param name="arg">Concatenate arguments</param>
    /// <param name="calledFromLocal"></param>
    [PunRPC]
    public override void ReplicatePlayerCommand(int commandId, string arg, bool calledFromLocal = true)
    {
        if (calledFromLocal)
        {
            photonView.RPC(nameof(ReplicatePlayerCommand), RpcTarget.All, commandId, arg, false);
            return;
        }

        onPlayerCommand?.Invoke(new PlayerCommandData()
        {
            CommandID = commandId,
            Arg = arg,
        });
    }

    /// <summary>
    /// Send a call to all other clients to sync a bullet
    /// </summary>
    [PunRPC]
    public override void ReplicateFire(GunType weaponType, Vector3 hitPosition, Vector3 inacuracity, bool calledFromLocal = true)
    {
        if (calledFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            photonView.RPC(nameof(ReplicateFire), RpcTarget.Others, weaponType, hitPosition, inacuracity, false);
#if MFPSTPV
            if (bl_CameraViewSettings.IsThirdPerson())
                PlayerReferences.playerAnimations?.PlayFireAnimation(weaponType);
#endif
            return;
        }

        if (CurrenGun == null) return;

        CurrenGun.OnNetworkFire(weaponType, new bl_NetworkGun.NetworkFireData()
        {
            HitPosition = hitPosition,
            Inaccuracy = inacuracity
        });
    }

    /// <summary>
    /// public method to send the RPC shot synchronization
    /// </summary>
    [PunRPC]
    public override void ReplicateGrenadeThrow(float t_spread, Vector3 pos, Quaternion rot, Vector3 direction, string id, bool calledFromLocal = true)
    {
        if (calledFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            photonView.RPC(nameof(ReplicateGrenadeThrow), RpcTarget.Others, new object[] { t_spread, pos, rot, direction, id, false });
            return;
        }

        if (CurrenGun == null) return;

        CurrenGun.GrenadeFire(t_spread, pos, rot, direction, id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectileData"></param>
    /// <param name="calledFromLocal"></param>    
    public override void ReplicateCustomProjectile(CustomProjectileData projectileData, bool calledFromLocal = true)
    {
        var netData = bl_UtilityHelper.CreatePhotonHashTable();
        netData.Add("position", projectileData.Origin);
        netData.Add("rotation", projectileData.Rotation);

        photonView.RPC(nameof(PhotonCustomProjectile), RpcTarget.Others, netData);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    [PunRPC]
    private void PhotonCustomProjectile(Hashtable data)
    {
        if (CurrenGun == null) return;

        CurrenGun?.FireCustomLogic(data);
        PlayerReferences.playerAnimations.PlayFireAnimation(CurrenGun.Info.Type);
    }

    /// <summary>
    /// Sync a player animation command
    /// </summary>
    /// <param name="command"></param>
    [PunRPC]
    public override void ReplicatePlayerAnimationCommand(PlayerAnimationCommands command, string arg = "", bool calledFromLocal = true)
    {
        if (!calledFromLocal)
        {
            PlayerReferences.playerAnimations.CustomCommand(command, arg, false);
        }
        else
        {
            photonView.RPC(nameof(ReplicatePlayerAnimationCommand), RpcTarget.All, command, arg, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockState"></param>
    public override void SetWeaponBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (!bl_PhotonNetwork.OfflineMode)
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, blockState);
        else
            RPCSetWBlocked(blockState);
    }

    [PunRPC]
    public void RPCSetWBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (isWeaponBlocked)
        {
            for (int i = 0; i < NetworkGuns.Count; i++)
            {
                NetworkGuns[i].gameObject.SetActive(false);
            }
        }
        else
        {
            CurrentTPVGun(false, true);
        }

        PlayerReferences.playerAnimations.BlockWeapons(blockState);
        currentGunID = -1;
    }

    [PunRPC]
    void SyncCustomizer(int weaponID, string line, PhotonMessageInfo info)
    {
        if (photonView.ViewID != info.photonView.ViewID) return;
#if CUSTOMIZER
        bl_NetworkGun ng = NetworkGuns.Find(x => x.LocalGun.GunID == weaponID);
        if (ng != null)
        {
            if (ng.GetComponent<bl_CustomizerWeapon>() != null)
            {
                ng.GetComponent<bl_CustomizerWeapon>().ApplyAttachments(line);
            }
            else
            {
                Debug.LogWarning("You have not setup the attachments in the TPWeapon: " + weaponID);
            }
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bl_NetworkGun GetCurrentNetworkWeapon()
    {
        return CurrenGun;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnSpectatorModeChanged(bool enterIn)
    {
        if (bl_SpectatorModeBase.IsSpectator && bl_GameData.CoreSettings.showNameplatesOnSpectator)
        {
            DrawName.SetActive(true);
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdatePosition()
    {
        if (m_PositionModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localPosition = m_PositionControl.UpdatePosition(m_Transform.localPosition);
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateRotation()
    {
        if (m_RotationModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localRotation = m_RotationControl.GetRotation(m_Transform.localRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void DoDrawEstimatedPositionError()
    {
        if (NetworkBodyState == PlayerState.InVehicle) return;

        Vector3 targetPosition = m_PositionControl.GetNetworkPosition();

        Debug.DrawLine(targetPosition, m_Transform.position, Color.red, 2f);
        Debug.DrawLine(m_Transform.position, m_Transform.position + Vector3.up, Color.green, 2f);
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
    }

    /// <summary>
    /// These values are synchronized to the remote objects if the interpolation mode
    /// or the extrapolation mode SynchronizeValues is used. Your movement script should pass on
    /// the current speed (in units/second) and turning speed (in angles/second) so the remote
    /// object can use them to predict the objects movement.
    /// </summary>
    /// <param name="speed">The current movement vector of the object in units/second.</param>
    /// <param name="turnSpeed">The current turn speed of the object in angles/second.</param>
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
    {
        m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, isWeaponBlocked ? 1 : 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetHeadLookPosition()
    {
        return PlayerReferences == null || PlayerReferences.playerIK == null
            ? HeadTarget.position + HeadTarget.forward
            : PlayerReferences.playerIK.HeadLookTarget.position;
    }

    private bl_FirstPersonControllerBase FpControler => PlayerReferences.firstPersonController;
    private bl_PlayerHealthManagerBase PlayerHealthManager => PlayerReferences.playerHealthManager;
}
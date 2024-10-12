using MFPS.Internal;
using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerAnimations : bl_PlayerAnimationsBase
{
    #region Public members
    [Tooltip("Override animation clips in the Animator controller when a specific trigger and value are detected, this allow us to easily change the animations for certain states without need to create new layers or state machines in the animator controller.")]
    [ScriptableDrawer] public bl_PlayerAnimationSettings settings;
    #endregion

    #region Public properties
    public bl_NetworkGun CurrentNetworkGun
    {
        get;
        set;
    }
    public bool useFootSteps { get; set; } = true;
    public float VelocityMagnitude
    {
        get;
        set;
    }
    #endregion

    #region Private members
    private RaycastHit footRay;
    private float reloadSpeed = 1;
    private PlayerState lastBodyState = PlayerState.Idle;
    private bl_Footstep footstep;
    private float deltaTime = 0.02f;
    private Transform m_Transform;
    private Dictionary<string, int> animatorHashes;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical, horizontal;
    private Transform PlayerRoot;
    private float turnAngle, smoothedTurnAngle;
    private float lastYRotation;
    private float movementSpeed;
    private float timeSinceLastMove = 0;
    private AnimatorOverrideController overrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;
    private bl_PlayerAnimationSettings.OverrideAnimationClips weaponTypeOverride;
    private Dictionary<AnimationClip, AnimationClip> defaultWeaponTyperOverrides;
    private static Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> cachedRACs = new();
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (PlayerReferences == null) return;

        m_Transform = transform;
        if (PlayerReferences != null) PlayerRoot = PlayerReferences.transform;
        if (useFootSteps)
        {
            footstep = PlayerReferences.firstPersonController.GetFootStep();
        }
        if (animatorHashes == null)
        {
            FetchHashes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_AnimatorReloadEvent.OnTPReload += OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AnimatorReloadEvent.OnTPReload -= OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        // cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new Dictionary<string, int>
        {
            { "BodyState", Animator.StringToHash("BodyState") },
            { "Vertical", Animator.StringToHash("Vertical") },
            { "Horizontal", Animator.StringToHash("Horizontal") },
            { "Speed", Animator.StringToHash("Speed") },
            { "Turn", Animator.StringToHash("Turn") },
            { "isGround", Animator.StringToHash("isGround") },
            { "UpperState", Animator.StringToHash("UpperState") },
            { "Move", Animator.StringToHash("Move") },
            { "GunType", Animator.StringToHash("GunType") },
            { "DoubleJump", Animator.StringToHash("Double Jump") },
        };

        /* if (cachedRACs.ContainsKey(Animator.runtimeAnimatorController))
         {
             // to avoid create a new instance of the same animator controller for every player
             // we will use a cached copy of it.
             Animator.runtimeAnimatorController = cachedRACs[Animator.runtimeAnimatorController];
         }
         else
         {*/
        // create a instance copy of the animator controller to avoid override the original one.
        RuntimeAnimatorController copy = Instantiate(Animator.runtimeAnimatorController);
        // cachedRACs.Add(Animator.runtimeAnimatorController, copy);
        Animator.runtimeAnimatorController = copy;
        //  }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTPReload(bool enter, Animator theAnimator, AnimatorStateInfo stateInfo)
    {
        if (theAnimator != Animator || CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null) return;

        float duration = CurrentNetworkGun.LocalGun != null ? CurrentNetworkGun.LocalGun.GetReloadTime() : CurrentNetworkGun.Info.ReloadTime;
        reloadSpeed = enter ? (stateInfo.length / duration) : 1;
        Animator.SetFloat(2036790888, reloadSpeed); // Hashcode: ReloadSpeed
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        deltaTime = Time.deltaTime;
        ControllerInfo();
        Animate();
        UpperControll();
        UpdateFootstep();
        DropPlayerAngle();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        if (BodyState == PlayerState.InVehicle)
        {
            Velocity = Vector3.zero;
            smoothedTurnAngle = movementSpeed = 0;
            movementSpeed = vertical = horizontal = 0;
            return;
        }

        if (PlayerRoot != null)
            LocalVelocity = PlayerRoot.InverseTransformDirection(Velocity);

        float lerp = deltaTime * settings.blendSmoothness;
        vertical = Mathf.Lerp(vertical, LocalVelocity.z, lerp);
        horizontal = Mathf.Lerp(horizontal, LocalVelocity.x, lerp);

        VelocityMagnitude = Velocity.magnitude;
        movementSpeed = Mathf.Lerp(movementSpeed, VelocityMagnitude, lerp);

        if (VelocityMagnitude > 0.1f)
        {
            timeSinceLastMove = Time.time;
        }

        turnAngle = Mathf.DeltaAngle(lastYRotation, PlayerRoot.localEulerAngles.y);
        // Calculate rotation speed in degrees per second
        float rotationSpeed = turnAngle / deltaTime;
        rotationSpeed = Mathf.InverseLerp(0, 180, Mathf.Abs(rotationSpeed));
        // if rotation less than 3 degrees per second, consider it's not rotating
        if (rotationSpeed < deltaTime * 3)
        {
            turnAngle = 0;
        }
        else
        {
            // Calculate angle divisor based on rotation speed
            float adjustedAngleDivisor = settings.turnAngleThreshold * (1 / rotationSpeed);

            adjustedAngleDivisor = Mathf.Max(adjustedAngleDivisor, 5);

            turnAngle = Mathf.Clamp(turnAngle / adjustedAngleDivisor, -1f, 1f);

        }

        smoothedTurnAngle = Mathf.Lerp(smoothedTurnAngle, turnAngle, lerp);
        if (BodyState != PlayerState.Idle || Time.time - timeSinceLastMove < 1)
        {
            smoothedTurnAngle = 0;
        }

        // cache the last rotation each 7 frames
        if (Time.frameCount % 7 == 0) lastYRotation = PlayerRoot.localEulerAngles.y;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (Animator == null)
            return;

        CheckPlayerStates();

        Animator.SetInteger(animatorHashes["BodyState"], (int)BodyState);
        Animator.SetFloat(animatorHashes["Vertical"], vertical);
        Animator.SetFloat(animatorHashes["Horizontal"], horizontal);
        Animator.SetFloat(animatorHashes["Speed"], movementSpeed);
        Animator.SetFloat(animatorHashes["Turn"], smoothedTurnAngle);
        Animator.SetBool(animatorHashes["isGround"], IsGrounded);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayerStates()
    {
        if (BodyState != lastBodyState)
        {
            if (lastBodyState == PlayerState.Sliding && BodyState != PlayerState.Sliding)
            {
                Animator.CrossFade(animatorHashes["Move"], 0.2f, 0);
            }
            if (BodyState == PlayerState.Sliding)
            {
                Animator.Play("Slide", 0, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Dropping))
            {
                Animator.Play("EmptyUpper", 1, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Gliding))
            {
                Animator.Play("EmptyUpper", 1, 0);
                Animator.CrossFade("gliding-1", 0.33f, 0);
            }

            if (OnExitPlayerState(PlayerState.Dropping))
            {
                m_Transform.localRotation = Quaternion.identity;
            }

            lastBodyState = BodyState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnEnterPlayerState(PlayerState playerState)
    {
        if (BodyState == playerState && lastBodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnExitPlayerState(PlayerState playerState)
    {
        if (lastBodyState == playerState && BodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControll()
    {
        int _fpState = (int)FPState;
        if (_fpState == 9) { _fpState = 1; }
        Animator.SetInteger(animatorHashes["UpperState"], _fpState);
    }

    /// <summary>
    /// 
    /// </summary>
    void DropPlayerAngle()
    {
        if (BodyState != PlayerState.Dropping) return;

        Vector3 pangle = m_Transform.localEulerAngles;
        float tilt = settings.dropTiltAngleCurve.Evaluate(Mathf.Clamp01(VelocityMagnitude / (PlayerReferences.firstPersonController.GetSpeedOnState(PlayerState.Dropping) - 10)));
        pangle.x = Mathf.Lerp(0, 70, tilt);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(pangle), deltaTime * 4);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    public override void CustomCommand(PlayerAnimationCommands command, string arg = "", bool callFromLocal = true)
    {
        // if it is called locally, sync the command to the network and return.
        if (callFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            PlayerReferences.playerNetwork.ReplicatePlayerAnimationCommand(command, arg);
            return;
        }

        if (!gameObject.activeSelf)
        {
            OnCustomCommand?.Invoke(command, arg);
            return;
        }

        if (animatorHashes == null) FetchHashes();

        switch (command)
        {
            case PlayerAnimationCommands.PlayDoubleJump:
                Animator.CrossFade(animatorHashes["DoubleJump"], 0.14f, 0);
                break;
            case PlayerAnimationCommands.OnFlashbang:
            case PlayerAnimationCommands.OnUnFlashed:
                Animator.SetBool("Flashed", command == PlayerAnimationCommands.OnFlashbang);
                break;
            case PlayerAnimationCommands.QuickMelee:
                int knifeGunId = int.Parse(arg);
                PlayerReferences.playerNetwork.TemporalySwitchTPWeapon(knifeGunId, 1000);
                Animator.Play("QuickFireKnife", 1, 0);
                break;
            case PlayerAnimationCommands.QuickGrenade:
                int grenadeGunId = int.Parse(arg);
                PlayerReferences.playerNetwork.TemporalySwitchTPWeapon(grenadeGunId, 1000);
                Animator.Play("QuickFireGrenade", 1, 0);
                break;
        }

        OnCustomCommand?.Invoke(command, arg);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void BlockWeapons(int blockState)
    {
        if (PlayerReferences == null || PlayerReferences.playerIK == null) return;

        bool baredHands = blockState == 1;

        // Do not control the arms with IK when the player is not using weapons.
        PlayerReferences.playerIK.ControlArmsWithIK = !baredHands;

        if (blockState != 2)
        {
            // -1 is the ID for play the bared arms animations in the player animator controller.
            int id = baredHands ? -1 : (int)cacheWeaponType;

            if (animatorHashes != null) Animator.SetInteger(animatorHashes["GunType"], id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        Animator.Play(hit, 2, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateFootstep()
    {
        if (!useFootSteps || BodyState == PlayerState.InVehicle || footstep == null) return;
        if (VelocityMagnitude < 0.3f) return;

        bool isClimbing = (BodyState == PlayerState.Climbing);
        if ((!IsGrounded && !isClimbing) || BodyState == PlayerState.Sliding)
            return;

        if (BodyState == PlayerState.Stealth)
        {
            if (footstep.settings != null) footstep.SetVolumeMuliplier(footstep.settings.stealthModeVolumeMultiplier);
        }
        else
        {
            footstep.SetVolumeMuliplier(1f);
        }

        footstep.UpdateStep(movementSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typ"></param>
    public override void PlayFireAnimation(GunType typ)
    {
        // Check if the network weapon is using custom animations
        if (CurrentNetworkGun != null && CurrentNetworkGun.useCustomPlayerAnimations)
        {
            if (!string.IsNullOrEmpty(CurrentNetworkGun.customFireAnimationName))
            {
                Animator.Play(CurrentNetworkGun.customFireAnimationName, 1, 0);
                return;
            }
        }

        switch (typ)
        {
            case GunType.Melee:
                Animator.Play("FireKnife", 1, 0);
                break;
            case GunType.Machinegun:
                Animator.Play("RifleFire", 1, 0);
                break;
            case GunType.Pistol:
                Animator.Play("PistolFire", 1, 0);
                break;
            case GunType.Launcher:
                Animator.Play("LauncherFire", 1, 0);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HitPlayer()
    {
        if (Animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            Animator.SetInteger("HitType", ht);
            Animator.SetTrigger("Hit");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateAnimatorParameters()
    {
        ControllerInfo();
        Animate();
        UpperControll();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetNetworkGun(GunType weaponType, bl_NetworkGun networkGun)
    {
        if (Animator == null || !Animator.gameObject.activeInHierarchy) return;

        cacheWeaponType = weaponType;
        CurrentNetworkGun = networkGun;

        if (animatorHashes == null) FetchHashes();

        if (networkGun != null && Animator != null)
        {
            Animator.SetInteger(animatorHashes["GunType"], networkGun.GetUpperStateID());
            Animator.Play("Equip", 1, 0);
        }

        if (settings != null)
        {
            // check if the current weapon type does need to override default animation clips, e.g the base sprint animation.
            if (settings.CheckOverrides(bl_PlayerAnimationSettings.OverrideAnimationClips.TriggerReason.WeaponType, networkGun.GetUpperStateID(), out var replacement))
            {
                weaponTypeOverride = replacement;
                ReplaceAnimationClip(replacement.DefaultClip, replacement.OverrideClip);
            }
            else
            {
                if (weaponTypeOverride != null)
                {
                    AnimationClip defaultClip = null;
                    if (defaultWeaponTyperOverrides != null && defaultWeaponTyperOverrides.ContainsKey(weaponTypeOverride.DefaultClip))
                    {
                        defaultClip = defaultWeaponTyperOverrides[weaponTypeOverride.DefaultClip];
                    }
                    // back to the default animation clip
                    ReplaceAnimationClip(weaponTypeOverride.DefaultClip, defaultClip);
                    weaponTypeOverride = null;
                }
            }
        }

        if (CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null)
        {
            reloadSpeed = 1;
        }

        // @TODO: Find the issue that cause Missing Reference exception when this conditional is not present.
        if (this != null)
        {
            // Do not control the arms with IK while the 'Equip' animation is playing.
            PlayerReferences.playerIK.ControlArmsWithIK = false;

            CancelInvoke(nameof(ResetHandsIK));
            Invoke(nameof(ResetHandsIK), 0.3f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="defaultName"></param>
    /// <param name="newClip"></param>
    public void ReplaceAnimationClip(AnimationClip defaultClip, AnimationClip newClip)
    {
        if (defaultClip == null) return;

        if (overrideController == null)
            overrideController = Animator.runtimeAnimatorController as AnimatorOverrideController;

        if (overrideClips == null)
        {
            overrideClips = new();
            overrideController.GetOverrides(overrideClips);
        }

        for (int i = 0; i < overrideClips.Count; i++)
        {
            var item = overrideClips[i];
            if (item.Key == null || item.Key.name != defaultClip.name) continue;

            defaultWeaponTyperOverrides ??= new();
            if (!defaultWeaponTyperOverrides.ContainsKey(item.Key))
            {
                defaultWeaponTyperOverrides.Add(item.Key, item.Value);
            }

            overrideClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(item.Key, newClip);
            break;
        }

        overrideController.ApplyOverrides(overrideClips);
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetHandsIK() { PlayerReferences.playerIK.ControlArmsWithIK = true; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GunType GetCurretWeaponType() { return cacheWeaponType; }

    private bl_PlayerReferences _playerReferences = null;
    private bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}
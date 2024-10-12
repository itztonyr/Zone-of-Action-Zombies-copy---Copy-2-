using MFPS.Audio;
using MFPS.PlayerController;
using MFPS.Runtime.Level;
using MFPSEditor;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class bl_FirstPersonController : bl_FirstPersonControllerBase
{
    #region Public members
    [Header("Settings")]
    public float WalkSpeed = 4.5f;
    public float runSpeed = 8;
    public float stealthSpeed = 1;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float crouchSpeed = 2;
    public float slideSpeed = 10;
    [FormerlySerializedAs("m_ClimbSpeed")]
    public float climbSpeed = 1f;
    [FormerlySerializedAs("m_JumpSpeed")]
    public float jumpSpeed;
    public float doubleJumpSpeed = 8;
    public bool canDoubleJump = true;
    public float acceleration = 9;
    public float maxSlopeAngle = 75;
	
    public float crouchTransitionSpeed = 0.25f;
    public float crouchHeight = 1.4f;

    public bool canSlide = true;
    [Range(0.2f, 1.5f)] public float slideTime = 0.75f;
    [Range(1, 12)] public float slideFriction = 10;
    [Range(0.1f, 2.5f)] public float slideCoolDown = 1.2f;
    [Range(5, 90)] public float slideSlopeLimit = 35;
    public float slideCameraTiltAngle = -22;

    [Range(0, 2)] public float JumpMinRate = 0.82f;
    public float jumpMomentumBooster = 2;
    public float momentunDecaySpeed = 5;
    [Range(0, 2)] public float AirControlMultiplier = 0.8f;
    public float m_StickToGroundForce;
    public float m_GravityMultiplier;

    [LovattoToogle] public bool RunFovEffect = true;
    public float runFOVAmount = 8;
    public bool canStealthMode = true;
    [Header("Falling")]
    [LovattoToogle] public bool FallDamage = true;
    [Range(0.1f, 5f)]
    public float SafeFallDistance = 3;
    [Range(3, 25)]
    public float DeathFallDistance = 15;
    public PlayerRunToAimBehave runToAimBehave = PlayerRunToAimBehave.StopRunning;
	
    [Header("Dropping")]
    public float dropControlSpeed = 25;
    public Vector2 dropTiltSpeedRange = new(20, 60);
    [Header("Mouse Look"), FormerlySerializedAs("m_MouseLook")]
    public MouseLook mouseLook;
    [FormerlySerializedAs("HeatRoot")]
    public Transform headRoot;
    public Transform CameraRoot;
    [Header("HeadBob")]
    [Range(0, 1.2f)] public float headBobMagnitude = 0.9f;
    public float headVerticalBobMagnitude = 0.4f;
    public LerpControlledBob m_JumpBob = new();
    [Header("FootSteps")]
    public bl_Footstep footstep;
    [ScriptableDrawer] public bl_AudioBank playerAudioBank;
    #endregion

    #region Public properties
    public CollisionFlags CollisionFlags { get; set; }
    public override Vector3 Velocity { get; set; }
    public override float VelocityMagnitude { get; set; }
    public override bool IsControlable { get; set; } = true;
    public bool OwnsRevive;
    #endregion

    #region Private members
    private bool jumpPressed;
    private Vector2 m_Input = new();
    private Vector3 targetDirection, moveDirection = Vector3.zero;
    private bool m_PreviouslyGrounded = true;
    private bool m_Jumping = false;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing, isAiming = false;
    private bl_Ladder m_Ladder;
#if MFPSM
    private bl_Joystick Joystick;
#endif
    private float PostGroundVerticalPos = 0;
    private bool isFalling = false;
    private int JumpDirection = 0;
    private float HigherPointOnJump;
    private CharacterController m_CharacterController;
    private float lastJumpTime = 0;
    private float WeaponWeight = 1;
    private bool hasTouchGround = false;
    private bool nextLandInmune = false;
    private Transform m_Transform;
    private readonly RaycastHit[] SurfaceRay = new RaycastHit[1];
    private Vector3 desiredMove, momentum = Vector3.zero;
    private float VerticalInput, HorizontalInput;
    private bool lastCrouchState = false;
    private float fallingTime = 0;
    private bool haslanding = false;
    private float capsuleRadious;
    private readonly Vector3 feetPositionOffset = new(0, 0.8f, 0);
    private float slideForce = 0;
    private float lastSlideTime = 0;
    private bl_PlayerReferences playerReferences;
    private Vector3 forwardVector = Vector3.forward;
    private PlayerState lastState = PlayerState.Idle;
    private bool forcedCrouch = false;
    private Vector3 surfaceNormal, surfacePoint = Vector3.zero;
    private float defaultStepOffset = 0.4f;
    private float desiredSpeed = 4;
    private float defaultHeight = 2;
    private bool overrideNextLandEvent = false;
    private bool holdToCrouch = true, holdToSprint = true;
    private bool isSprintActive = false;
    private bool isDoubleJump = false;
    private float runFov = 0;
    private Coroutine crouchCoroutine;
    #endregion

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!photonView.IsMine)
            return;

        base.Awake();
        m_Transform = transform;
        TryGetComponent(out playerReferences);
        m_CharacterController = playerReferences.characterController;
#if MFPSM
        Joystick = FindObjectOfType<bl_Joystick>();
#endif
        defaultCameraRPosition = CameraRoot.localPosition;
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        mouseLook.Init(m_Transform, headRoot);
        lastJumpTime = Time.time;
        defaultStepOffset = m_CharacterController.stepOffset;
        capsuleRadious = m_CharacterController.radius * 0.5f;
        defaultHeight = m_CharacterController.height;
        IsControlable = bl_MatchTimeManagerBase.HaveTimeStarted();
        OnGameSettingsChanged();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.onRoundEnd += OnRoundEnd;
        bl_EventHandler.onChangeWeapon += OnChangeWeapon;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged += OnAimChange;
#if MFPSM
        bl_TouchHelper.OnCrouch += OnCrouchClicked;
        bl_TouchHelper.OnJump += DoJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.onRoundEnd -= OnRoundEnd;
        bl_EventHandler.onChangeWeapon -= OnChangeWeapon;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged -= OnAimChange;
#if MFPSM
        bl_TouchHelper.OnCrouch -= OnCrouchClicked;
        bl_TouchHelper.OnJump -= DoJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;
        playerReferences.cameraMotion.AddCameraFOV(0, runFov);

        if (Finish) return;

        MovementInput();
        CheckStates();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnLateUpdate()
    {
        UpdateMouseLook();

        if (m_CharacterController == null || !m_CharacterController.enabled) return;
        if (Finish)
        {
            OnGameOverMove();
            return;
        }

        moveDirection = Vector3.Lerp(moveDirection, targetDirection, acceleration * Time.deltaTime);

        // apply the movement direction in the character controller
        // apply the movement in LateUpdate to avoid the camera jerky/jitter effect when move and rotate the player camera.
        CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    public void FixedUpdate()
    {
        if (Finish || m_CharacterController == null || !m_CharacterController.enabled)
            return;

        GroundDetection();

        if (isClimbing && m_Ladder != null)
        {
            //climbing control
            OnClimbing();
        }
        else
        {
            //player movement
            Move();
        }
    }
    #endregion

    /// <summary>
    /// Triggered when the state of this player controller has changed.
    /// </summary>
    private void OnStateChanged(PlayerState from, PlayerState to)
    {
        if (from == PlayerState.Crouching || to == PlayerState.Crouching)
        {
            DoCrouchTransition();
        }
        else if (from == PlayerState.Sliding || to == PlayerState.Sliding)
        {
            DoCrouchTransition();
        }
        bl_EventHandler.DispatchLocalPlayerStateChange(from, to);
    }

    /// <summary>
    /// Handle the player input key/buttons for the player controller
    /// </summary>
    void MovementInput()
    {
        //if player focus is in game
        if (bl_GameInput.IsCursorLocked && !bl_GameData.isChatting)
        {
            //determine the player speed
            GetInput(out float s);
            desiredSpeed = Mathf.Lerp(desiredSpeed, s, Time.deltaTime * 8);
            Speed = desiredSpeed;
        }
        else if (State != PlayerState.Sliding)//if player is not focus in game
        {
            m_Input = Vector2.zero;
        }

        jumpPressed |= bl_GameInput.Jump();
        if (State == PlayerState.Sliding)
        {
            slideForce -= Time.deltaTime * slideFriction;
            Speed = slideForce;
            // cancel sliding if press the jump button
            if (bl_GameInput.Jump())
            {
                State = PlayerState.Jumping;
            }
            else return;
        }

        if (bl_UtilityHelper.isMobile) return;

        if (bl_GameInput.Jump() && State != PlayerState.Crouching && !isDoubleJump && (Time.time - lastJumpTime) > JumpMinRate)
        {
            DoJump();
        }

        if (State != PlayerState.Jumping && State != PlayerState.Climbing)
        {
            if (forcedCrouch) return;
            if (holdToCrouch)
            {
                Crounching = bl_GameInput.Crouch();
                if (Crounching != lastCrouchState)
                {
                    OnCrouchChanged();
                    lastCrouchState = Crounching;
                }
            }
            else
            {
                if (bl_GameInput.Crouch(GameInputType.Down))
                {
                    Crounching = !Crounching;
                    OnCrouchChanged();
                }
            }
        }
    }

    /// <summary>
    /// Check when the player is in a surface (not jumping or falling)
    /// </summary>
    void GroundDetection()
    {
        //if the player has touch the ground after falling
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            OnLand();
        }
        else if (m_PreviouslyGrounded && !m_CharacterController.isGrounded)//when the player start jumping
        {
            PostGroundVerticalPos = m_Transform.position.y;
            if (!isFalling)
            {
                isFalling = true;
                fallingTime = Time.time;
            }
        }

        if (isFalling)
        {
            VerticalDirectionCheck();
        }

        // if the player start falling (not from a jump)
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded && !nextLandInmune)
        {
            targetDirection.y = 0f;
        }

        // if there's something blocking the player head
        if (forcedCrouch)
        {
            if ((Time.frameCount % 10) == 0)
            {
                if (!IsHeadHampered())
                {
                    forcedCrouch = false;
                    State = PlayerState.Idle;
                }
            }
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    /// <summary>
    /// Apply the movement direction to the CharacterController
    /// </summary>
    void Move()
    {
        //if the player is touching the surface
        if (m_CharacterController.isGrounded)
        {
            OnSurface();
            //vertical resistance
            moveDirection.y = -m_StickToGroundForce;
            hasTouchGround = true;
        }
        else//if the player is not touching the ground
        {
            //if the player is dropping
            if (State == PlayerState.Dropping)
            {
                //handle the air movement in different process
                OnDropping();
                return;
            }
            else if (State == PlayerState.Gliding)
            {
                //handle the gliding movement in different function
                OnGliding();
                return;
            }

            OnAir();
        }
    }

    /// <summary>
    /// Control the player when is in a surface
    /// </summary>
    void OnSurface()
    {
        // always move along the camera forward as it is the direction that it being aimed at
        // desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);

        desiredMove.Set(m_Input.x, 0.0f, m_Input.y);
        desiredMove = m_Transform.TransformDirection(desiredMove);

        // get a normal for the surface that is being touched to move along it
        Physics.SphereCastNonAlloc(m_Transform.localPosition, capsuleRadious, Vector3.down, SurfaceRay, m_CharacterController.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        //determine the movement angle based in the normal of the current player surface
        desiredMove = Vector3.ProjectOnPlane(desiredMove, SurfaceRay[0].normal);
        targetDirection.x = desiredMove.x * Speed;
        targetDirection.z = desiredMove.z * Speed;

        SlopeControl();
    }

    /// <summary>
    /// Control the player when is in air (not dropping nor gliding)
    /// </summary>
    void OnAir()
    {
        desiredMove = (m_Transform.forward * Mathf.Clamp01(m_Input.y)) + (m_Transform.right * m_Input.x);
        desiredMove += momentum;

        targetDirection += m_GravityMultiplier * Time.fixedDeltaTime * Physics.gravity;
        targetDirection.x = desiredMove.x * Speed * AirControlMultiplier;
        targetDirection.z = desiredMove.z * Speed * AirControlMultiplier;

        momentum = Vector3.Lerp(momentum, Vector3.zero, Time.fixedDeltaTime * momentunDecaySpeed);
    }

    /// <summary>
    /// Called when the player hit a surface after falling/jumping
    /// </summary>
    void OnLand()
    {
        if (isClimbing) return;

        if (!overrideNextLandEvent)
        {
            //land camera effect
            StartCoroutine(m_JumpBob.DoBobCycle());

            isFalling = false;
            float fallDistance = CalculateFall();
            if (fallDistance > 0.05f) bl_EventHandler.DispatchPlayerLandEvent(fallDistance);
        }
        else overrideNextLandEvent = false;

        haslanding = true;
        JumpDirection = 0;
        targetDirection.y = 0f;
        m_Jumping = false;
        isDoubleJump = false;
        fallingTime = 0;
        HigherPointOnJump = m_Transform.position.y;
        m_CharacterController.stepOffset = defaultStepOffset;
        if (State != PlayerState.Crouching) State = PlayerState.Idle;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCrouchChanged()
    {
        if (Crounching)
        {
            State = PlayerState.Crouching;

            //Slide implementation
            if (canSlide && VelocityMagnitude > WalkSpeed && GetLocalVelocity().z > 0.1f)
            {
                DoSlide();
            }
        }
        else
        {
            if (!IsHeadHampered())
            {
                State = PlayerState.Idle;
            }
            else forcedCrouch = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoCrouchTransition()
    {
        if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
        crouchCoroutine = StartCoroutine(CrouchTransition());
        IEnumerator CrouchTransition()
        {
            bool isCrouch = Crounching || State == PlayerState.Sliding;
            float height = isCrouch ? crouchHeight : defaultHeight;
            Vector3 center = new(0, height * 0.5f, 0);
            Vector3 cameraPosition = CameraRoot.localPosition;
            Vector3 verticalCameraPos = isCrouch ? new Vector3(cameraPosition.x, defaultCameraRPosition.y - (defaultHeight - height), cameraPosition.z) : defaultCameraRPosition;

            float originHeight = m_CharacterController.height;
            Vector3 originCenter = m_CharacterController.center;
            playerReferences.cameraMotion.AddCameraPosition(-1, verticalCameraPos);
            if (State != PlayerState.Sliding) PlayPlayerAudio("crouch", 0.3f, Crounching ? 1 : 0.95f);

            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / crouchTransitionSpeed;
                m_CharacterController.height = Mathf.Lerp(originHeight, height, d);
                m_CharacterController.center = Vector3.Lerp(originCenter, center, d);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Make the player jump
    /// </summary>
    public override void DoJump()
    {
        if (isDoubleJump)
        {
            return;
        }

        // if already jump, this is a double jump
        if (State == PlayerState.Jumping)
        {
            if (!canDoubleJump) return;

            isDoubleJump = true;
            momentum = desiredMove * jumpMomentumBooster;
            moveDirection.y = doubleJumpSpeed;
            PlayPlayerAudio("double-jump");
            playerReferences.playerAnimations.CustomCommand(PlayerAnimationCommands.PlayDoubleJump);
            bl_EventHandler.Player.onLocalDoubleJump?.Invoke();
        }
        else
        {
            // first jump
            momentum = desiredMove * jumpMomentumBooster;
            moveDirection.y = jumpSpeed;
            PlayPlayerAudio("jump");
        }

        targetDirection.y = moveDirection.y;
        m_Jumping = true;
        if (State == PlayerState.Sliding) mouseLook.SetTiltAngle(0);
        Crounching = false;
        State = PlayerState.Jumping;
        lastJumpTime = Time.time;
        m_CharacterController.stepOffset = 0;
        CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Make the player slide
    /// </summary>
    public override void DoSlide()
    {
        if ((Time.time - lastSlideTime) < slideTime * slideCoolDown) return;//wait the equivalent of one extra slide before be able to slide again
        if (m_Jumping) return;

        Vector3 startPosition = m_Transform.position - feetPositionOffset + (m_Transform.forward * m_CharacterController.radius);
        if (Physics.Linecast(startPosition, startPosition + m_Transform.forward)) return;//there is something in front of the feet's

        State = PlayerState.Sliding;
        slideForce = slideSpeed;//slide force will be continually decreasing
        Speed = slideSpeed;
        mouseLook.SetTiltAngle(slideCameraTiltAngle);
        PlayPlayerAudio("slide");
        mouseLook.UseOnlyCameraRotation();
        this.InvokeAfter(slideTime, () =>
        {
            if (Crounching && !bl_UtilityHelper.isMobile)
                State = PlayerState.Crouching;
            else if (State != PlayerState.Jumping)
            {
                State = PlayerState.Idle;
                Crounching = false;
            }

            DoCrouchTransition();
            lastSlideTime = Time.time;
            mouseLook.SetTiltAngle(0);
            mouseLook.PortBodyOrientationToCamera();
        });
    }

    /// <summary>
    /// Detect slope limit and apply slide physics.
    /// </summary>
    void SlopeControl()
    {
        float angle = Vector3.Angle(Vector3.up, surfaceNormal);

        if (State == PlayerState.Sliding && angle >= slideSlopeLimit)
        {
            State = PlayerState.Idle;
            Crounching = false;
        }

        if (angle <= m_CharacterController.slopeLimit || angle >= maxSlopeAngle) return;

        Vector3 direction = Vector3.up - (surfaceNormal * Vector3.Dot(Vector3.up, surfaceNormal));
        float speed = slideSpeed + 1 + Time.deltaTime;

        targetDirection += direction * -speed;
        targetDirection.y -= surfacePoint.y;
    }

    /// <summary>
    /// Make the player dropping
    /// </summary>
    public override void DoDrop()
    {
        if (isGrounded)
        {
            Debug.Log("Can't drop when player is in a surface");
            return;
        }
        State = PlayerState.Dropping;
    }

    /// <summary>
    /// Called each frame when the player is dropping (fall control is On)
    /// </summary>
    void OnDropping()
    {
        //get the camera upside down angle
        float tilt = Mathf.InverseLerp(0, 90, mouseLook.VerticalAngle);
        //normalize it
        tilt = Mathf.Clamp01(tilt);
        if (mouseLook.VerticalAngle <= 0 || mouseLook.VerticalAngle >= 180) tilt = 0;
        //get the forward direction of the player camera
        desiredMove = headRoot.forward * Mathf.Clamp01(m_Input.y);
        if (desiredMove.y > 0) desiredMove.y = 0;

        //calculate the drop speed based in the upside down camera angle
        float dropSpeed = Mathf.Lerp(m_GravityMultiplier * dropTiltSpeedRange.x, m_GravityMultiplier * dropTiltSpeedRange.y, tilt);
        targetDirection = dropSpeed * Time.fixedDeltaTime * Physics.gravity;
        //if the player press the vertical input -> add velocity in the direction where the camera is looking at
        targetDirection += desiredMove * dropControlSpeed;

        //apply the movement direction in the character controller
        CollisionFlags = m_CharacterController.Move(targetDirection * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Make the player glide
    /// </summary>
    public override void DoGliding()
    {
        if (isGrounded)
        {
            Debug.Log("Can't gliding when player is in a surface");
            return;
        }
        State = PlayerState.Gliding;
    }

    /// <summary>
    /// Called each frame when the player is gliding
    /// </summary>
    void OnGliding()
    {
        desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);
        //how much can the player control the player when is in air.
        float airControlMult = AirControlMultiplier * 5;
        //fall gravity amount
        float gravity = m_GravityMultiplier * 15;

        targetDirection = gravity * Time.fixedDeltaTime * Physics.gravity;
        targetDirection.x = desiredMove.x * Speed * airControlMult;
        targetDirection.z = desiredMove.z * Speed * airControlMult;

        CollisionFlags = m_CharacterController.Move(targetDirection * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnClimbing()
    {
        if (m_Ladder.Status == bl_Ladder.LadderStatus.Detaching)
        {
            if (!m_Ladder.Exiting)
            {
                m_Ladder.Exiting = true;
                bool wasControllable = IsControlable;
                IsControlable = false;

                StartCoroutine(MoveTo(m_Ladder.GetNearestExitPosition(m_Transform), () =>
                {
                    SetActiveClimbing(false);
                    m_Ladder.JumpOut();
                    m_Ladder = null;
                    IsControlable = true;
                    bl_EventHandler.onPlayerLand(0.5f);
                }));
            }
        }
        else
        {
            desiredMove = m_Ladder.transform.rotation * forwardVector * m_Input.y;
            targetDirection.y = desiredMove.y * climbSpeed;
            targetDirection.x = desiredMove.x * climbSpeed;
            targetDirection.z = desiredMove.z * climbSpeed;

            if (jumpPressed)
            {
                SetActiveClimbing(false);
                m_Ladder.JumpOut();
                m_Ladder = null;

                targetDirection = -m_Transform.forward * 20;
                targetDirection.y = jumpSpeed;
                lastJumpTime = Time.time;
                jumpPressed = false;
            }
            CollisionFlags = m_CharacterController.Move(targetDirection * Time.smoothDeltaTime);

            if (m_Ladder != null) m_Ladder.WatchLimits();
        }
    }

    /// <summary>
    /// The restricted movement when the match is ended
    /// </summary>
    private void OnGameOverMove()
    {
        moveDirection.y += Physics.gravity.y * m_GravityMultiplier * Time.deltaTime;
        CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Math behind the fall damage calculation
    /// </summary>
    private float CalculateFall()
    {
        float fallDistance = HigherPointOnJump - m_Transform.position.y;
        if (JumpDirection == -1)
        {
            float normalized = PostGroundVerticalPos - m_Transform.position.y;
            fallDistance = Mathf.Abs(normalized);
        }

        if (FallDamage && hasTouchGround && haslanding)
        {
            if (nextLandInmune)
            {
                nextLandInmune = false;
                return fallDistance;
            }
            if ((Time.time - fallingTime) <= 0.4f)
            {
                if (fallDistance > 0.05f) bl_EventHandler.DispatchPlayerLandEvent(0.2f);
                return fallDistance;
            }

            float ave = fallDistance / DeathFallDistance;
            if (fallDistance > SafeFallDistance && bl_GameData.CoreSettings.allowFallDamage && State != PlayerState.InVehicle)
            {
                int damage = Mathf.FloorToInt(ave * 100);
                var damageData = new DamageData()
                {
                    Damage = damage,
                    From = bl_PhotonNetwork.NickName,
                    Cause = DamageCause.FallDamage,
                    OriginPosition = m_Transform.position - m_Transform.TransformVector(new Vector3(0, 5, 1)),
                };
                playerReferences.playerHealthManager.DoDamage(damageData);
            }

            PlayLandingSound(ave);
            fallingTime = Time.time;
        }
        else PlayLandingSound(1);
        return fallDistance;
    }

    /// <summary>
    /// Check in which vertical direction is the player translating to
    /// </summary>
    void VerticalDirectionCheck()
    {
        if (m_Transform.position.y == PostGroundVerticalPos) return;

        //if the direction has not been decided yet
        if (JumpDirection == 0)
        {
            //is the player below or above from the surface he was?
            // 1 = above (jump), -1 = below (falling)
            JumpDirection = (m_Transform.position.y > PostGroundVerticalPos) ? 1 : -1;
        }
        else if (JumpDirection == 1)//if the player jump
        {
            //but not start falling
            if (m_Transform.position.y < PostGroundVerticalPos)
            {
                //get the higher point he reached jumping
                HigherPointOnJump = PostGroundVerticalPos;
            }
            else//if still going up
            {
                PostGroundVerticalPos = m_Transform.position.y;
            }
        }
        else//if the player was falling without jumping
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetInput(out float outputSpeed)
    {
        if (!IsControlable)
        {
            m_Input = Vector2.zero;
            outputSpeed = 0;
            return;
        }

        // Read input
        HorizontalInput = bl_GameInput.Horizontal;
        VerticalInput = bl_GameInput.Vertical;

#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            HorizontalInput = Joystick.Horizontal;
            VerticalInput = Joystick.Vertical;
            VerticalInput *= 1.25f;
        }
#endif
        PlayerState currentState = State;

        if (currentState == PlayerState.Sliding)
        {
            VerticalInput = 1;
            HorizontalInput = 0;
        }

        m_Input.Set(HorizontalInput, VerticalInput);

        float inputMagnitude = m_Input.magnitude;
        bool wasJump = currentState == PlayerState.Jumping;
        //if the player is dropping, the speed is calculated in the dropping function
        if (currentState == PlayerState.Dropping || currentState == PlayerState.Gliding)
        {
            outputSpeed = 0;
            return;
        }

        if (currentState != PlayerState.Climbing && currentState != PlayerState.Sliding)
        {
            if (inputMagnitude > 0 && currentState != PlayerState.Crouching)
            {
                if (VelocityMagnitude > 0)
                {
                    float forwardVelocity = GetLocalVelocity().z;
                    if (!bl_UtilityHelper.isMobile)
                    {
                        if (holdToSprint) isSprintActive = bl_GameInput.Run();
                        else
                        {
                            if (bl_GameInput.Run(GameInputType.Down))
                            {
                                isSprintActive = !isSprintActive;
                            }
                        }

                        // On standalone builds, walk/run speed is modified by a key press.
                        // keep track of whether or not the character is walking or running
                        if (isSprintActive && forwardVelocity > 0.1f)
                        {
                            if (runToAimBehave == PlayerRunToAimBehave.AimWhileRunning)
                                State = PlayerState.Running;
                            else if (runToAimBehave == PlayerRunToAimBehave.StopRunning)
                                State = isAiming ? PlayerState.Walking : PlayerState.Running;
                        }
                        else if (canStealthMode && bl_GameInput.Stealth() && VelocityMagnitude > 0.1f)
                        {
                            State = PlayerState.Stealth;
                        }
                        else
                        {
                            State = PlayerState.Walking;
                        }
                    }
                    else
                    {
                        if (VerticalInput > 1 && forwardVelocity > 0.1f)
                        {
                            State = PlayerState.Running;
                        }
                        else if (canStealthMode && VerticalInput > 0.05f && VerticalInput <= 0.15f)
                        {
                            State = PlayerState.Stealth;
                        }
                        else
                        {
                            State = PlayerState.Walking;
                        }
                    }
                }
                else
                {
                    if (currentState != PlayerState.Jumping)
                    {
                        State = PlayerState.Idle;
                    }
                }

            }
            else if (m_CharacterController.isGrounded)
            {
                if (currentState != PlayerState.Jumping && currentState != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }

        if (Crounching)
        {
            outputSpeed = (State == PlayerState.Crouching) ? crouchSpeed : runSpeed;
            if (State == PlayerState.Sliding)
            {
                outputSpeed += 1;
            }
        }
        else
        {
            // set the desired speed to be walking or running
            outputSpeed = (State == PlayerState.Running && m_CharacterController.isGrounded) ? runSpeed : WalkSpeed;
            if (State == PlayerState.Stealth)
            {
                outputSpeed = stealthSpeed;
            }
        }
        // normalize input if it exceeds 1 in combined length:
        if (inputMagnitude > 1)
        {
            m_Input.Normalize();
        }

        if (RunFovEffect)
        {
            float rf = (State == PlayerState.Running && m_CharacterController.isGrounded) ? runFOVAmount : 0;
            runFov = Mathf.Lerp(runFov, rf, Time.deltaTime * 6);
        }

        if (wasJump) { State = PlayerState.Jumping; }
    }

    /// <summary>
    /// Force the player stop this frame
    /// </summary>
    public override void Stop()
    {
        m_Input = Vector2.zero;
        moveDirection = Vector3.zero;
        desiredMove = Vector3.zero;
        targetDirection = Vector3.zero;
        State = PlayerState.Idle;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void PlayFootStepSound()
    {
        if (State == PlayerState.Sliding || footstep == null || Finish) return;
        if (!m_CharacterController.isGrounded && !isClimbing)
            return;

        if (!isClimbing)
        {
            if (State == PlayerState.Stealth || State == PlayerState.Crouching)
            {
                if (footstep.settings != null) footstep.SetVolumeMuliplier(footstep.settings.stealthModeVolumeMultiplier);
            }
            else footstep.SetVolumeMuliplier(1f);

            footstep.DetectAndPlaySurface();
        }
        else
        {
            footstep.PlayStepForTag("Generic");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void AddForce(Vector3 force, bool fallDamageInmune = false)
    {
        nextLandInmune = fallDamageInmune;
        moveDirection = force;
        targetDirection = force;

        CollisionFlags = m_CharacterController.Move(moveDirection * Time.deltaTime);
    }

#if MFPSM
    /// <summary>
    /// 
    /// </summary>
    void OnCrouchClicked()
    {
        Crounching = !Crounching;
        OnCrouchChanged();
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateMouseLook()
    {
        mouseLook.Update();

        if (bl_GameInput.InputFocus != MFPSInputFocus.Player) return;

        if (!isClimbing)
        {
            mouseLook.UpdateLook(m_Transform, headRoot);
        }
        else
        {
            mouseLook.UpdateLook(m_Transform, headRoot, m_Ladder);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckStates()
    {
        if (lastState == State) return;
        OnStateChanged(lastState, State);
        lastState = State;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayLandingSound(float vol = 1)
    {
        vol = Mathf.Clamp(vol, 0.05f, 1);
        PlayPlayerAudio("landing", vol);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioBankKey"></param>
    public void PlayPlayerAudio(string audioBankKey, float customVolume = -1, float pitch = 1)
    {
        if (playerAudioBank == null) return;

        playerAudioBank.PlayAudioInSource(m_AudioSource, audioBankKey, customVolume, pitch);
    }

    /// <summary>
    /// Check if the player has a obstacle above the head
    /// </summary>
    /// <returns></returns>
    public bool IsHeadHampered()
    {
        Vector3 origin = m_Transform.localPosition + m_CharacterController.center + (0.5F * m_CharacterController.height * Vector3.up);
        float dist = 2.05f - m_CharacterController.height;
        return Physics.Raycast(origin, Vector3.up, dist);
    }

    #region External Events
    void OnRoundEnd()
    {
        Finish = true;
        IsControlable = false;
        Stop();
    }

    void OnChangeWeapon(int id)
    {
        isAiming = false;
        var currentWeapon = playerReferences.gunManager.GetCurrentWeapon();
        WeaponWeight = currentWeapon != null ? currentWeapon.WeaponWeight : 0;
    }

    void OnMatchStart()
    {
        IsControlable = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameSettingsChanged()
    {
        mouseLook.FetchSettings();
        holdToCrouch = !((bool)bl_MFPS.Settings.GetSettingOf("Toggle Crouch"));
        holdToSprint = !((bool)bl_MFPS.Settings.GetSettingOf("Toggle Sprint"));
    }

    void OnAimChange(bool aim)
    {
        isAiming = aim;
        mouseLook.OnAimChange(aim);
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void SetActiveClimbing(bool active)
    {
        isClimbing = active;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        if (isClimbing) bl_InputInteractionIndicator.ShowIndication(bl_Input.GetButtonName("Jump"), bl_GameTexts.JumpOffLadder);
        else bl_InputInteractionIndicator.SetActiveIfSame(false, bl_GameTexts.JumpOffLadder);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator MoveTo(Vector3 position, Action onFinish)
    {
        if (m_Transform == null) m_Transform = transform;

        float t = 0;
        Vector3 from = m_Transform.localPosition;
        m_CharacterController.enabled = false;
        while (t < 1)
        {
            t += Time.deltaTime / 0.5f;
            m_Transform.localPosition = Vector3.Lerp(from, position, t);
            yield return null;
        }
        m_CharacterController.enabled = true;
        onFinish?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    private void CheckLadderTrigger(Collider other)
    {
        // if the collider doesn't have a parent, it's not a valid ladder
        if (isClimbing || other.transform.parent == null)
            return;

        var ladder = other.GetComponentInParent<bl_Ladder>();
        if (ladder == null || !ladder.CanUse)
            return;

        // Enter in a ladder trigger

        Stop();
        m_Ladder = ladder;
        // setup the in and out position based on the player height
        ladder.SetUpBounds(playerReferences);

        bool wasControllable = IsControlable;
        IsControlable = false;
        nextLandInmune = true;
        jumpPressed = false;

        SetActiveClimbing(true);

        // get the position to automatically translate the player to start climbing/down-climbing
        Vector3 startPos = ladder.GetAttachPosition(other, m_CharacterController.height);
        overrideNextLandEvent = true;
        // move the player to the start position
        StartCoroutine(MoveTo(startPos, () =>
        {
            // after finish the position adjustment
            IsControlable = wasControllable;
            ladder.Status = bl_Ladder.LadderStatus.Climbing;

            // now the movement will be handled in OnClimbing() function
        }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        CheckLadderTrigger(other);
    }

    /// <summary>
    ///
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        surfaceNormal = hit.normal;
        surfacePoint = hit.point;
        /// Enable this if you want player controller apply force on contact to rigidbodys
        /// is commented by default for performance matters.
        /* Rigidbody body = hit.collider.attachedRigidbody;
         //dont move the rigidbody if the character is on top of it
         if (m_CollisionFlags == CollisionFlags.Below)
         {
             return;
         }

         if (body == null || body.isKinematic)
         {
             return;
         }
         body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);*/
    }

    internal float _speed = 0;
    public float Speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            float min = 1.75f;
            if (State == PlayerState.Stealth)
            {
                min = 1;
            }
            _speed = Mathf.Max(_speed, min);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetCurrentSpeed()
    {
        return Speed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetSpeedOnState(PlayerState playerState, bool includeModifiers)
    {
        return playerState switch
        {
            PlayerState.Walking => includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed,
            PlayerState.Running => includeModifiers ? runSpeed - WeaponWeight : runSpeed,
            PlayerState.Crouching => includeModifiers ? crouchSpeed - WeaponWeight : crouchSpeed,
            PlayerState.Dropping => dropTiltSpeedRange.y,
            _ => includeModifiers ? WalkSpeed - WeaponWeight : WalkSpeed,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override PlayerRunToAimBehave GetRunToAimBehave()
    {
        return runToAimBehave;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override MouseLookBase GetMouseLook()
    {
        return mouseLook;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bl_Footstep GetFootStep()
    {
        return footstep;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vertical"></param>
    /// <returns></returns>
    public override float GetHeadBobMagnitudes(bool vertical)
    {
        return vertical ? headVerticalBobMagnitude : headBobMagnitude;
    }

    public Vector3 GetLocalVelocity() => m_Transform.InverseTransformDirection(Velocity);
    public Vector3 MovementDirection => targetDirection;
    public override bool isGrounded { get { return m_CharacterController.isGrounded; } }

    [SerializeField]
    public class LerpControlledBob
    {
        public float BobDuration;
        public float BobAmount;

        private float m_Offset = 0f;

        // provides the offset that can be used
        public float Offset()
        {
            return m_Offset;
        }

        public IEnumerator DoBobCycle()
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(0f, BobAmount, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // make it move back to neutral
            t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            m_Offset = 0f;
        }
    }
}
using MFPS.Runtime.Motion;
using MFPSEditor;
using UnityEngine;

public class bl_CameraMotion : bl_CameraMotionBase
{
    [ScriptableDrawer] public bl_CameraMotionSettings motionSettings;

    #region Private members
    private bool wiggleMotion = true;
    private bool breathingMotion = false;
    private float m_breatingAmplitude;
    private float m_startBreathingTime;
    private Vector3 breathRotation = Vector3.zero;
    private float sideTilt = 0;
    private Quaternion rotationMotion;
    private bool isSliding = false;
    private bl_SpringTransform spring;
    private bl_Spring zoomSpring;
    private bl_PlayerReferences playerRefs;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        playerRefs = GetComponentInParent<bl_PlayerReferences>();
        m_breatingAmplitude = motionSettings.breatheAmplitude;
        rotationMotion = CachedTransform.localRotation;
        spring = motionSettings.spring;
        zoomSpring = motionSettings.zoomSpring;
        if (spring != null) spring = spring.InitAndGetInstance(CachedTransform);
        if (zoomSpring != null)
        {
            zoomSpring = zoomSpring.InitAndGetInstance();
            zoomSpring.Current = playerRefs.DefaultCameraFOV;
            zoomSpring.Target = playerRefs.DefaultCameraFOV;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPlayerLand += OnSmallImpact;
        bl_EventHandler.onLocalPlayerStateChanged += OnPlayerStateChanged;
        bl_EventHandler.Player.onLocalDoubleJump += OnDoubleJump;
        wiggleMotion = bl_GameData.CoreSettings.playerViewRolling;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPlayerLand -= OnSmallImpact;
        bl_EventHandler.onLocalPlayerStateChanged -= OnPlayerStateChanged;
        bl_EventHandler.Player.onLocalDoubleJump -= OnDoubleJump;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ZoomControl();
        if (!bl_GameInput.IsCursorLocked) return;

        Breathe();
        Wiggle();

        if (spring != null)
        {
            spring.RotationSpring.Target = breathRotation;
            spring.Update();
        }

        if (zoomSpring != null)
        {
            zoomSpring.UpdateSpring(Time.deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ZoomControl()
    {
        if (zoomSpring == null) return;

        playerRefs.playerCamera.fieldOfView = zoomSpring.Current;
    }

    /// <summary>
    /// 
    /// </summary>
    void Wiggle()
    {
        if (!wiggleMotion) return;

        float rollAngle = -bl_GameInput.MouseX * motionSettings.tiltAngle;
        if (isSliding) rollAngle = 0;
        rollAngle = Mathf.Clamp(rollAngle, -motionSettings.tiltAngle, motionSettings.tiltAngle);

        if (motionSettings.sideMoveEffector > 0)
        {
            sideTilt = Mathf.Lerp(sideTilt, -bl_GameInput.Horizontal * motionSettings.sideMoveEffector, Time.deltaTime * (motionSettings.smooth * 2));
            rollAngle += sideTilt;
        }

        breathRotation.z = bl_GameInput.Aim() ? rollAngle * motionSettings.aimMultiplier : rollAngle;
        rotationMotion = Quaternion.Slerp(rotationMotion, Quaternion.Euler(breathRotation), Time.smoothDeltaTime * motionSettings.smooth);
    }

    /// <summary>
    /// 
    /// </summary>
    void Breathe()
    {
        if (!breathingMotion) return;

        float theta = (Time.time - m_startBreathingTime) / motionSettings.breathePeriod;
        float vertical = m_breatingAmplitude * Mathf.Sin(theta);
        float horizontal = m_breatingAmplitude * Mathf.Cos(theta * 0.5f);
        breathRotation = (Vector3.up * vertical) + (Vector3.right * horizontal);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    public override void AddCameraRotation(int layer, Vector3 rotation, bool autoReset = false)
    {
        if (spring != null) spring.RotationSpring.SetTargetLayer(layer, rotation, autoReset);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="position"></param>
    /// <param name="autoReset"></param>
    public override void AddCameraPosition(int layer, Vector3 position, bool autoReset = false)
    {
        if (spring != null) spring.PositionSpring.SetTargetLayer(layer, position, autoReset);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="fov"></param>
    public override void AddCameraFOV(int layer, float extraFov, bool autoReset = false)
    {
        if (zoomSpring == null) return;

        zoomSpring.SetTargetLayer(layer, extraFov, autoReset);
        zoomSpring.SetMultiplier(zoomSpring.Target <= 16 ? 5f : 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveBreathing(bool active, float amplitude = 0)
    {
        breathingMotion = active;

        if (!breathingMotion)
        {
            m_breatingAmplitude = motionSettings.breatheAmplitude;
            breathRotation = Vector3.zero;
        }
        else
        {
            m_breatingAmplitude = amplitude;
            m_startBreathingTime = Time.time;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnSmallImpact(float impactAmount)
    {
        if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount, 0, 0), true);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDoubleJump()
    {
        if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount * 0.5f, 0, 0), true);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerStateChanged(PlayerState from, PlayerState to)
    {
        isSliding = to == PlayerState.Sliding;
        if (to == PlayerState.Jumping)
        {
            if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount * 0.5f, 0, 0), true);
        }
    }
}
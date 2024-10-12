using MFPS.Runtime.Motion;
using MFPSEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraMotionSettings", menuName = "MFPS/Camera/Motion Settings")]
public class bl_CameraMotionSettings : ScriptableObject
{
    [ScriptableDrawer] public bl_SpringTransform spring;
    [ScriptableDrawer] public bl_Spring zoomSpring;

    [Header("Wiggle")]
    public float smooth = 4f;
    public float tiltAngle = 6f;
    public float sideMoveEffector = 3;
    public float aimMultiplier = 0.5f;

    [Header("Breathe")]
    public float breatheAmplitude = 0.5f;
    public float breathePeriod = 1f;

    [Header("Fall Effect")]
    [Range(0.01f, 1.0f)]
    public float DownAmount = 8;
}
using UnityEngine;

public abstract class bl_CameraMotionBase : bl_MonoBehaviour
{
    /// <summary>
    /// Add an extra camera rotation to the camera motion
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void AddCameraRotation(int layer, Vector3 rotation, bool autoReset = false);

    /// <summary>
    /// Add an extra camera rotation to the camera motion
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void AddCameraPosition(int layer, Vector3 position, bool autoReset = false);

    /// <summary>
    /// Add an extra camera FOV to the camera motion
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="extraFov"></param>
    /// <param name="autoReset"></param>
    public abstract void AddCameraFOV(int layer, float extraFov, bool autoReset = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    /// <param name="amplitude"></param>
    public abstract void SetActiveBreathing(bool active, float amplitude = 0);
}
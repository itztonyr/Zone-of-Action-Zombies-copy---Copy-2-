using MFPS.Audio;
using MFPS.Internal.Scriptables;
using MFPSEditor;
using UnityEngine;

public class bl_Footstep : MonoBehaviour
{
    public bl_FootStepsLibrary footStepsLibrary;
    [ScriptableDrawer] public bl_FootstepSettings settings;

    private AudioSource audioSource;
    private RaycastHit m_raycastHit;
    private Transform m_Transform;
    private string surfaceTag;
    private float nextRate = 0;
    private float lastStepTime = 0;
    private float volumeMultiplier = 1;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        if (!TryGetComponent(out audioSource))
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        if (bl_AudioController.Instance != null)
        {
            audioSource.maxDistance = bl_AudioController.Instance.maxFootstepDistance;
            audioSource.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
            audioSource.minDistance = bl_AudioController.Instance.maxFootstepDistance * 0.05f;
        }
        audioSource.spatialize = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateStep(float speed)
    {
        if (settings == null) return;
        if (Time.time - lastStepTime <= nextRate) return;

        DetectAndPlaySurface();

        if (speed > (settings.walkSpeed + 1)) nextRate = settings.runStepRate;
        else if (speed < (settings.walkSpeed - 1)) nextRate = settings.crouchStepRate;
        else nextRate = settings.walkStepRate;

        lastStepTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DetectAndPlaySurface()
    {
        if (Physics.Raycast(m_Transform.position, -Vector3.up, out m_raycastHit, 5, settings.surfaceLayers, QueryTriggerInteraction.Ignore))
        {
            surfaceTag = m_raycastHit.transform.tag;
            if (m_raycastHit.transform.TryGetComponent<bl_TerrainSurfaces>(out var ts))
            {
                surfaceTag = ts.GetSurfaceTag(m_raycastHit.point);
            }
            PlayStepForTag(surfaceTag);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepForTag(string tag)
    {
        AudioClip step = GetStepSoundFor(tag);
        PlayStepSound(step);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepSound(AudioClip clip)
    {
        if (footStepsLibrary != null)
            audioSource.pitch = Random.Range(footStepsLibrary.pitchRange.x, footStepsLibrary.pitchRange.y);
        audioSource.clip = clip;
        audioSource.volume = settings.stepsVolume * volumeMultiplier;
        audioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetVolumeMuliplier(float multiplier)
    {
        volumeMultiplier = multiplier;
    }

    /// <summary> 
    /// Get a random step sound clip for the given tag
    /// </summary>
    /// <returns></returns>
    public AudioClip GetStepSoundFor(string tag)
    {
        if (footStepsLibrary == null) return null;

        var stepsGroup = footStepsLibrary.GetGroupFor(tag);
        return stepsGroup.GetRandomClip();
    }
}
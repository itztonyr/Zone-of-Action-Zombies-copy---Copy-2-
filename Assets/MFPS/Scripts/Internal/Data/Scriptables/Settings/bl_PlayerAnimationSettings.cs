using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Animation Settings", menuName = "MFPS/Player/Animation Settings")]
public class bl_PlayerAnimationSettings : ScriptableObject
{
    public AnimationCurve dropTiltAngleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float blendSmoothness = 10;
    public float turnAngleThreshold = 5f;
    public List<OverrideAnimationClips> overrideClips;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bl_PlayerAnimations"></param>
    public bool CheckOverrides(OverrideAnimationClips.TriggerReason reason, int value, out OverrideAnimationClips replacement)
    {
        foreach (var item in overrideClips)
        {
            if (item.Reason != reason || item.Reason == OverrideAnimationClips.TriggerReason.None) continue;

            if (item.WhenValue == value)
            {
                replacement = item;
                return true;
            }
        }

        replacement = null;
        return false;
    }

    [Serializable]
    public class OverrideAnimationClips
    {
        public TriggerReason Reason;
        public int WhenValue = 0;
        public AnimationClip DefaultClip;
        public AnimationClip OverrideClip;

        public enum TriggerReason
        {
            None = 0,
            WeaponType = 1,
            UpperState = 2,
        }
    }
}
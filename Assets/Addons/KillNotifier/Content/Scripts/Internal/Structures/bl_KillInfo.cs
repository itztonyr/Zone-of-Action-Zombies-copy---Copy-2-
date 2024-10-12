using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.KillStreak
{
    [Serializable]
    public class KillStreakInfo
    {
        [Tooltip("The text that will be displayed in the kill notifier")]
        public string KillName = "Kill 0";
        [Tooltip("Extra score granted when reached this streak")]
        public int ExtraScore = 0;
        [Tooltip("Skip this notification?")]
        [LovattoToogle] public bool Skip = false;
        [Header("References")]
        [SpritePreview(50)] public Sprite KillIcon;
        public AudioClip KillClip;

        [NonSerialized] public int killID = 0;
        [NonSerialized] public KillInfo info = null;
        [NonSerialized] public List<string> specials = new();

        public void AddSpecial(string key)
        {
            if (specials.Contains(key)) return;
            specials.Add(key);
        }
    }

    [Serializable]
    public class SpecialKillNotifier
    {
        public string key;
        public KillStreakInfo info;
    }

    [Serializable]
    public enum KillNotifierTextType
    {
        KillName,
        KillCount,
    }
}
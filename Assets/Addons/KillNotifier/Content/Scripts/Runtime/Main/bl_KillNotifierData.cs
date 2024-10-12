using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.KillStreak
{
    public class bl_KillNotifierData : ScriptableObject
    {
        [Header("Streaks")]
        [Reorderable] public List<KillStreakInfo> KillsStreaks = new List<KillStreakInfo>();
        public List<SpecialKillNotifier> specialNotifications;

        [Header("Settings")]
        [Range(1, 10)] public float TimeToShow = 3;
        public float timeBetweenSpecialBadge = 0.12f;
        [Range(0.01f, 1)] public float volumeMultiplier = 1;
        public float longShotDistanceThreshold = 50;
        [Tooltip("If true, the kill notification will show instantly as the elimination happens remplacing any currently showing, if false, it will wait for any current notification to finish before showing the next in the queue.")]
        [LovattoToogle] public bool OverrideOnNewStreak = true;
        [LovattoToogle] public bool prioretizeHeadShotNotification = false;
        public KillNotifierTextType killNotifierTextType = KillNotifierTextType.KillCount;
        public string killCountFormat = "{0} Kill";

        public KillStreakInfo GetKillStreakInfo(int killNumber)
        {
            killNumber = killNumber - 1;
            if (killNumber > (KillsStreaks.Count - 1)) { return KillsStreaks[KillsStreaks.Count - 1]; }
            return KillsStreaks[killNumber];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool TryGetNotification(string key, out KillStreakInfo info)
        {
            foreach (SpecialKillNotifier notification in Instance.specialNotifications)
            {
                if (notification.key == key)
                {
                    info = notification.info;
                    return true;
                }
            }
            info = null;
            return false;
        }

        private static bl_KillNotifierData m_Data;
        public static bl_KillNotifierData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("KillNotifierData", typeof(bl_KillNotifierData)) as bl_KillNotifierData;
                }
                return m_Data;
            }
        }
    }
}
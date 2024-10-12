using UnityEngine;

namespace MFPS.Addon.KillStreak
{
    public static class bl_KillNotifierUtils
    {

        public static bl_KillStreakManager GetManager
        {
            get
            {
                bl_KillStreakManager m = GameObject.FindObjectOfType<bl_KillStreakManager>();
                return m;
            }
        }

        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }
    }
}
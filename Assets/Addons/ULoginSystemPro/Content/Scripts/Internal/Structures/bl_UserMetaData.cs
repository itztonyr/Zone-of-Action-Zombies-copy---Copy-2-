using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    [Serializable]
    public class bl_UserMetaData
    {
        public RawData rawData = new RawData();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void AddTodayDateEntry(string key)
        {
            var todayTick = DateTime.UtcNow.Ticks;
            AddDateEntry(key, todayTick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="date"></param>
        public void AddDateEntry(string key, long dateInTicks)
        {
            if (rawData.dateEntries == null) { rawData.dateEntries = new List<DateEntries>(); }

            int index = rawData.dateEntries.FindIndex(x => x.key == key);
            if (index != -1)
            {
                rawData.dateEntries[index].date = dateInTicks;
                return;
            }

            var entry = new DateEntries
            {
                key = key,
                date = dateInTicks
            };
            rawData.dateEntries.Add(entry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetDateEntry(string key, out DateEntries entry)
        {
            entry = null;
            if (rawData.dateEntries == null) { return false; }

            int index = rawData.dateEntries.FindIndex(x => x.key == key);
            if (index == -1) { return false; }

            entry = rawData.dateEntries[index];
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string data = JsonUtility.ToJson(this);
            return data;
        }

        [Serializable]
        public class RawData
        {
            public string WeaponsLoadouts;
            public int ClassKit = 0;
            public int Avatar = 0;
            public int CallingCard = 0;
            public List<DateEntries> dateEntries;
            public string emotes;
        }

        [Serializable]
        public class DateEntries
        {
            public string key;
            public long date; // ticks

            /// <summary>
            /// 
            /// </summary>
            /// <param name="date"></param>
            /// <returns></returns>
            public bool IsSameDayAsToday()
            {
                DateTime dateFromTicks = new DateTime(date, DateTimeKind.Utc);
                DateTime utcNow = DateTime.UtcNow.Date;
                return dateFromTicks.Date == utcNow;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public DateTime GetDate()
            {
                return new DateTime(date, DateTimeKind.Utc);
            }
        }
    }
}
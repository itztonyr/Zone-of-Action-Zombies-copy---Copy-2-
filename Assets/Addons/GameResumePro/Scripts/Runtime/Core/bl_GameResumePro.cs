using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GameResumePro : bl_MatchFinishResumeBase
    {
        public int returnToLobbyIn = 60;
        public bl_GameResumeProUI resumeUI;

        public List<StatPropertie> stats = new List<StatPropertie>();

        private bool firstSpawn = false;
        private int killStreak, highestStreak = 0;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            bl_EventHandler.onLocalKill += OnLocalKill;
            bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDie;
            bl_EventHandler.onLocalPlayerFire += OnLocalPlayerFire;
            bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
            bl_EventHandler.onLocalPlayerHitEnemy += OnLocalPlayerHitAnEnemy;
            bl_EventHandler.onLocalKillAssist += OnLocalKillAssist;
            resumeUI.gameObject.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            bl_EventHandler.onLocalKill -= OnLocalKill;
            bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDie;
            bl_EventHandler.onLocalPlayerFire -= OnLocalPlayerFire;
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
            bl_EventHandler.onLocalPlayerHitEnemy -= OnLocalPlayerHitAnEnemy;
            bl_EventHandler.onLocalKillAssist -= OnLocalKillAssist;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetStat(string key, float value) => SetStat(key, (int)value);

        /// <summary>
        /// 
        /// </summary>
        public void SetStat(string key, int value)
        {
            int index = stats.FindIndex(x => x.Key == key);
            if (index == -1)
            {
                stats.Add(new StatPropertie()
                {
                    Key = key,
                    Value = value
                });
                return;
            }
            stats[index].Value += value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetStat(string key)
        {
            int index = stats.FindIndex(x => x.Key == key);
            if (index == -1) return 0;

            return stats[index].Value;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CollectMatchData()
        {
            var playTime = (int)Time.time - GetStat("start-time");
            SetStat("play-time", playTime);

            resumeUI.FetchData(this);
            SavePlayerData();
            bl_WaitingRoom.SetWaitingState(bl_WaitingRoom.WaitingState.Waiting);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetActive(bool active)
        {
            resumeUI.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SavePlayerData()
        {

            int totalScore = GetStat("total-score-gained");
            int totalCoins = GetStat("total-coins-gained");

            SaveMatchInDataBase(totalCoins, totalScore, true);

            bl_EventHandler.Match.onSaveMatchData?.Invoke(totalScore);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalKill(KillInfo killInfo)
        {
            if (killInfo.Killer != bl_PhotonNetwork.LocalPlayer.NickName) return;

            SetStat($"kw-{killInfo.KillMethod}", 1);
            SetStat($"kp-{killInfo.Killed}", 1);
            if (killInfo.byHeadShot) SetStat("hs", 1);
            killStreak++;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalPlayerDie()
        {
            SetStat("deaths", 1);
            highestStreak = Mathf.Max(killStreak, highestStreak);
            killStreak = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalPlayerFire(int gunID)
        {
            SetStat("bf", 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enemyName"></param>
        void OnLocalPlayerHitAnEnemy(MFPSHitData hitData)
        {
            SetStat("hits", 1);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalPlayerSpawn()
        {
            if (firstSpawn) return;

            SetStat("start-time", Time.time);
#if LM
            SetStat("start-score", bl_LevelManager.Instance.GetRuntimeLocalScore());
#endif
            firstSpawn = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        void OnLocalKillAssist(bl_EventHandler.KillAssistData data)
        {
            SetStat("assists", 1);
        }

        public class StatPropertie
        {
            public string Key;
            public int Value;
        }
    }
}
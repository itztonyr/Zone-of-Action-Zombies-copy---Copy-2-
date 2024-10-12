using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GameResumeProUnlocks : MonoBehaviour
    {
        public int showUnlockItemCount = 5;
        public bl_GameResumePro gameResumePro;
        public bl_GMUnlockItem unlockUITemplate;
        public RectTransform unlocksPanel;

        private bool itemsFetched = false;
        private List<UnlockItemInfo> unlockList = new List<UnlockItemInfo>();

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            FetchUnlocks();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchUnlocks()
        {
            if (itemsFetched) return;
#if LM
            var currentScore = gameResumePro.GetStat("start-score");
            var unlocks = bl_LevelManager.Instance.GetUnlocksList(currentScore, showUnlockItemCount);
            var currentLevelID = bl_LevelManager.Instance.GetRuntimeLocalScore();

            foreach (var item in unlocks)
            {
                var unlock = new UnlockItemInfo();
                unlock.Name = item.Name;
                unlock.Level = item.UnlockLevel;
                unlock.Preview = item.Preview;
                unlock.Unlocked = item.UnlockLevel <= currentLevelID;
                unlockList.Add(unlock);
            }
            InstanceItemsUI();
#endif
            itemsFetched = true;
        }

        /// <summary>
        /// 
        /// </summary>
        void InstanceItemsUI()
        {
            unlockUITemplate.gameObject.SetActive(false);
            for (int i = 0; i < unlockList.Count; i++)
            {
                var go = Instantiate(unlockUITemplate.gameObject) as GameObject;
                go.transform.SetParent(unlocksPanel, false);
                go.SetActive(true);
                go.GetComponent<bl_GMUnlockItem>().Set(unlockList[i]);
            }
        }

        [Serializable]
        public class UnlockItemInfo
        {
            public string Name;
            public int Level;
            public Sprite Preview;
            public bool Unlocked = false;
        }
    }
}
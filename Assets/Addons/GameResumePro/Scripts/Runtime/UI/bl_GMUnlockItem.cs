using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GMUnlockItem : MonoBehaviour
    {
        public Image iconImg;
        public TextMeshProUGUI unlockLevelText;
        public GameObject blockUI;

        /// <summary>
        /// 
        /// </summary>
        public void Set(bl_GameResumeProUnlocks.UnlockItemInfo info)
        {
            iconImg.sprite = info.Preview;
            unlockLevelText.text = $"LEVEL {info.Level}";
            blockUI.SetActive(!info.Unlocked);
        }
    }
}
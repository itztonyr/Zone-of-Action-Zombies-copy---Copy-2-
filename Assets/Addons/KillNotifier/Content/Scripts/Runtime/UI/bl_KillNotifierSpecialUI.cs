using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.KillStreak
{
    public class bl_KillNotifierSpecialUI : MonoBehaviour
    {
        [SerializeField] private Image iconImg = null;
        [SerializeField] private TextMeshProUGUI badgeNameText = null;
        [SerializeField] private AudioSource audioSource = null;

        public string SpecialKey { get; set; }
        private KillStreakInfo currentInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        public void Setup(string key, KillStreakInfo info)
        {
            if (info == null) return;

            currentInfo = info;
            SpecialKey = key;
            iconImg.sprite = info.KillIcon;
            if (badgeNameText != null)
            {
                badgeNameText.text = info.ExtraScore > 0 ? $"{info.KillName}\n+{info.ExtraScore}" : info.KillName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (currentInfo == null) return;

            if (audioSource != null && currentInfo.KillClip != null)
            {
                audioSource.clip = currentInfo.KillClip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SetAtSiblingPosition(int index) => transform.SetSiblingIndex(index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}
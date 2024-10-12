using UnityEngine;
using TMPro;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GameResumeProStat : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI tagText;
        public GameObject tagUI;

        public void SetStat(string name, string value, string tag = "")
        {
            nameText.text = name.ToUpper();
            valueText.text = value;
            tagUI.SetActive(!string.IsNullOrEmpty(tag));
            tagText.text = tag;
        }
    }
}
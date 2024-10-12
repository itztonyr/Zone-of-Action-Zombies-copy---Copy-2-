using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GRLevelBox : MonoBehaviour
    {
        public Image levelIcon;
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelNumberText;
        public GameObject effectUI;

#if LM
        public void Set(MFPS.Addon.LevelManager.LevelInfo level)
        {
            if (levelIcon != null) levelIcon.sprite = level.Icon;
            if (levelNameText != null) levelNameText.text = level.Name;
            if (levelNumberText != null) levelNumberText.text = level.LevelID.ToString();
            effectUI.SetActive(false);
            effectUI.SetActive(true);
        }
#endif
    }
}
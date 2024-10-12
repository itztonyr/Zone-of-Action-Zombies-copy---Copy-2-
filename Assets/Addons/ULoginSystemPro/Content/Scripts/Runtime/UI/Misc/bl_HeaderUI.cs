using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin.UI
{
    public class bl_HeaderUI : MonoBehaviour
    {      
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI title = null;
        [SerializeField] private TextMeshProUGUI gameTitle = null;
        [SerializeField] private Button backButton = null;

        /// <summary>
        /// 
        /// </summary>
        public void SetupFor(bl_ULoginUIBase.MenuWindow window)
        {
            if(bl_DataBaseUtils.IsEnumFlagPresent(window.HeaderToShow, bl_ULoginUIBase.MenuWindow.HeaderParts.Empty))
            {
                content.SetActive(false);
            }
            else
            {
                content.SetActive(true);
                if(title != null)
                {
                    if(!string.IsNullOrEmpty(window.DisplayName)) title.text = window.DisplayName.ToUpper();
                    title.gameObject.SetActive(bl_DataBaseUtils.IsEnumFlagPresent(window.HeaderToShow, bl_ULoginUIBase.MenuWindow.HeaderParts.Title));
                }

                if (gameTitle != null)
                {
                    gameTitle.gameObject.SetActive(bl_DataBaseUtils.IsEnumFlagPresent(window.HeaderToShow, bl_ULoginUIBase.MenuWindow.HeaderParts.GameTitle));
                }

                if(window.onBack.GetPersistentEventCount() > 0)
                {
                    backButton.onClick.RemoveAllListeners();
                    backButton.onClick.AddListener(() => window.onBack?.Invoke());
                    backButton.gameObject.SetActive(true);
                }
                else
                {
                    backButton.gameObject.SetActive(false);
                }
            }
        }
    }
}
using MFPS.Internal.Structures;
using MFPS.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Customizer
{
    public class bl_CustomizerInfoButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;
        public Image weaponIcon;
        public TextMeshProUGUI lockedText;
        public Button button;
        public GameObject lockedUI;
        [SerializeField] private GameObject selectedUI = null;
        [SerializeField] private bl_MFPSCoinPriceUI coinPricesUI = null;

        private bl_CustomizerInfoButton[] AllButtons;
        private bl_Customizer customizerWeapon;
        private bool isUnlocked = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        public void Init(bl_Customizer weapon)
        {
            customizerWeapon = weapon;
            var info = customizerWeapon.GetWeaponInfo();
            m_Text.text = customizerWeapon.WeaponName;
            weaponIcon.sprite = info.GunIcon;

            isUnlocked = info.Unlockability.IsUnlocked(customizerWeapon.GunID());
            if (!isUnlocked)
            {
                var reason = info.Unlockability.GetLockReason(customizerWeapon.GunID());
                if (reason == MFPSItemUnlockability.LockReason.NoPurchased || reason == MFPSItemUnlockability.LockReason.NoPurchasedAndLevel)
                {
                    coinPricesUI.SetPrice(info.Unlockability).SetActive(true);
                }
                else coinPricesUI.SetActive(false);

                if (reason == MFPSItemUnlockability.LockReason.Level || reason == MFPSItemUnlockability.LockReason.NoPurchasedAndLevel)
                {
                    lockedText.text = $"LEVEL {info.Unlockability.UnlockAtLevel}";
                }
                else lockedText.text = "";

                lockedUI.SetActive(true);
            }
            else
            {
                button.interactable = true;
                lockedUI.SetActive(false);
            }
            Deselect();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnSelect()
        {
            if (!isUnlocked) return;

            if (AllButtons == null || AllButtons.Length <= 0) { AllButtons = transform.parent.GetComponentsInChildren<bl_CustomizerInfoButton>(); }

            bl_CustomizerManager.Instance.ShowCustomizerWeapon(customizerWeapon);

            foreach (bl_CustomizerInfoButton b in AllButtons)
            {
                b.Deselect();
            }
            button.interactable = false;
            if (selectedUI != null) selectedUI.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Deselect()
        {
            button.interactable = true;
            if (selectedUI != null) selectedUI.SetActive(false);
        }
    }
}
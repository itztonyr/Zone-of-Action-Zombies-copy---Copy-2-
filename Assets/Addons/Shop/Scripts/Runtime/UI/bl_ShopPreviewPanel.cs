using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_ShopPreviewPanel : MonoBehaviour
    {
        public GameObject InfoPanel;
        public GameObject BuyPreviewButton;
        public TextMeshProUGUI PreviewNameText;
        public bl_PriceUI previewPriceUI;
        public Image[] PreviewBars;
        public List<ItemTypeUI> PreviewIcons;

        private ShopProductData infoPreviewData = null;

        [Serializable]
        public class ItemTypeUI
        {
            public ShopItemType ItemType;
            public Image IconImage;
            public bool ShowStats;

            public void SetActive(bool active)
            {
                if (IconImage == null) return;

                IconImage.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Preview(ShopProductData info, bool isOwned, bool canPurchase = true)
        {
            InfoPanel.SetActive(true);
            infoPreviewData = info;
            PreviewNameText.text = info.Name.ToUpper();
            previewPriceUI.ShowPrices(info.UnlockabilityInfo);
            SetupIcon(info);

            if (info.Type == ShopItemType.Weapon)
            {
                PreviewBars[0].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "DAMAGE:";
                PreviewBars[0].fillAmount = (float)info.GunInfo.Damage / 100f;
                PreviewBars[1].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "FIRE RATE:";
                PreviewBars[1].fillAmount = info.GunInfo.FireRate / 1f;
                PreviewBars[2].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "ACCURACY";
                PreviewBars[2].fillAmount = (float)info.GunInfo.Accuracy / 5f;
                PreviewBars[3].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "WEIGHT:";
                PreviewBars[3].fillAmount = (float)info.GunInfo.Weight / 4f;
            }
            else if (info.Type == ShopItemType.PlayerSkin)
            {
#if PSELECTOR
                var pdm = info.PlayerSkinInfo.Prefab.GetComponent<bl_PlayerHealthManager>();
                var fpc = info.PlayerSkinInfo.Prefab.GetComponent<bl_FirstPersonControllerBase>();
                PreviewBars[0].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "HEALTH:";
                PreviewBars[0].fillAmount = pdm.health / 125;
                PreviewBars[1].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "SPEED:";
                PreviewBars[1].fillAmount = fpc.GetSpeedOnState(PlayerState.Walking) / 5;
                PreviewBars[2].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "REGENERATION:";
                PreviewBars[2].fillAmount = pdm.RegenerationSpeed / 5;
                PreviewBars[3].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "NOISE:";
                PreviewBars[3].fillAmount = 0.9f;
#endif
            }

            BuyPreviewButton.SetActive(!isOwned && canPurchase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        private void SetupIcon(ShopProductData product)
        {
            foreach (var item in PreviewIcons)
            {
                item.SetActive(false);
            }
            var ui = PreviewIcons.Find(x => x.ItemType == product.Type);
            if (ui == null) ui = PreviewIcons[0];

            ui.IconImage.sprite = product.GetIcon();
            ui.SetActive(true);
            InfoPanel.SetActive(ui.ShowStats);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CheckoutPreview()
        {
#if ULSP
            if (!bl_DataBase.IsUserLogged)
            {
                bl_ShopNotification.Instance?.Show("You need an account to make purchases.").Hide(3);
                Debug.LogWarning("You has to be login in order to make a purchase.");
                return;
            }
#endif
            bl_ShopManager.Instance.PreviewItem(infoPreviewData, BuyPreviewButton.GetComponent<RectTransform>().position);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_ShopItemUI : bl_ShopItemUIBase, IPointerEnterHandler, IPointerClickHandler
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI typeText;
        public List<IconImage> m_icons;
        public RectTransform BuyButton;
        public GameObject OwnedUI;
        public GameObject BuyUI;
        public GameObject levelBlockUI;
        public bl_PriceUI priceUI;
        public MonoBehaviour[] oneTimeUsed;
        public int ID { get; set; } = 0;
        public ShopItemType TypeID;

        private ShopProductData Info;
        private bool isOwned = false;
        private bool canPurchase = true;

        [Serializable]
        public class IconImage
        {
            public ShopItemType ItemType;
            public Image IconImg;

            public void SetActive(bool active)
            {
                if (IconImg == null) return;
                IconImg.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void Setup(ShopProductData data)
        {
            priceUI?.SetActive(false);
            Info = data;
            ID = data.ID;

            TypeID = data.Type;
            NameText.text = Info.Name.ToUpper();
            string typeName = data.Type.ToString();
            typeName = string.Concat(typeName.Select(x => System.Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            typeText.text = typeName.Localized(data.Type.ToString().ToLower()).ToUpper();
            LayoutRebuilder.ForceRebuildLayoutImmediate(NameText.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(typeText.transform.parent.GetComponent<RectTransform>());
            //that's kinda dirty but it works :)
            foreach (MonoBehaviour b in oneTimeUsed) { Destroy(b); }

            // If this item is free
            // IsUnlocked will return True if the local player is not logged or is a guest.
            // So a further conditional is required for that scenario.
            if (Info.UnlockabilityInfo.IsUnlocked(ID))
            {
#if ULSP
                // if the user has not been logged or if it's a guest, don't let him select the weapons.
                if ((!bl_DataBase.IsUserLogged || bl_DataBase.IsGuest) && Info.UnlockabilityInfo.CanBePurchased())
                {
                    ShowBlockUI();
                }
                else
                {
                    // Means that the weapon is unlocked for this player
                    ShowOwnedUI();
                }
#else
                ShowOwnedUI();
#endif
            }
            else
            {
                if (Info.UnlockabilityInfo.CanBePurchased())
                {
                    ShowBlockUI();
                }
                else
                {
                    // If this object only can be unlocked by level up.
                    ShowBlockUI(false);
                }
            }

            SetupIcon(Info);
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowBlockUI(bool requirePurchase = true)
        {
            priceUI.ShowPrices(Info.UnlockabilityInfo);
            priceUI.SetActive(requirePurchase);
            BuyUI.SetActive(requirePurchase);
            isOwned = false;
            canPurchase = requirePurchase;
            BuyButton.gameObject.SetActive(requirePurchase);
            OwnedUI.SetActive(false);
            if (levelBlockUI != null) levelBlockUI.SetActive(!requirePurchase);
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowOwnedUI()
        {
            BuyUI.SetActive(false);
            isOwned = true;
            canPurchase = false;
            GetComponent<Selectable>().interactable = false;
            OwnedUI.SetActive(true);
            if (levelBlockUI != null) levelBlockUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        public void SetupIcon(ShopProductData product)
        {
            foreach (var item in m_icons)
            {
                item.SetActive(false);
            }
            var image = m_icons.Find(x => x.ItemType == product.Type);
            if (image == null) image = m_icons[0];
            image.IconImg.sprite = product.GetIcon();
            image.SetActive(true);
        }

        public void OnBuy()
        {
#if ULSP && SHOP

            if (!bl_DataBase.IsUserLogged)
            {
                bl_ShopNotification.Instance?.Show("You need an account to make purchases.").Hide(3);
                Debug.LogWarning("You has to be login in order to make a purchase.");
                return;
            }
            else
            {
                if (bl_UserWallet.HasFundsFor(Info.Price))
                {
                    bl_ShopManager.Instance.PreviewItem(Info, BuyButton.position);
                }
                else
                {
                    bl_ShopManager.Instance.NoCoinsWindow.SetActive(true);
                }
            }
#else
                        Debug.LogWarning("You need have ULogin Pro enabled to use this addon");
            return;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (bl_ShopData.Instance.itemPreviewTrigger != bl_ShopData.ItemPreviewTrigger.OnPointEnter) return;

            bl_ShopManager.Instance.Preview(Info, isOwned, canPurchase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (bl_ShopData.Instance.itemPreviewTrigger != bl_ShopData.ItemPreviewTrigger.OnClick) return;

            bl_ShopManager.Instance.Preview(Info, isOwned, canPurchase);
        }
    }
}
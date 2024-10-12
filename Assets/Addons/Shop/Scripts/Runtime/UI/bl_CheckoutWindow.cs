using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_CheckoutWindow : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI itemNameText = null;
        [SerializeField] private TextMeshProUGUI itemDescriptionText = null;
        [SerializeField] private Image iconImage = null;
        [SerializeField] private bl_PriceUI priceUI = null;
        [SerializeField] private GameObject[] windows = null;

        public ShopProductData ItemInCart { get; private set; }

        private bool isBusy = false;
        private Action onCompletePurchase;
        private Action<int> onConfirmed;
        private string defaultDescription;

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            onCompletePurchase = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Checkout(ShopProductData item, Action onComplete = null)
        {
            ItemInCart = item;
            onCompletePurchase = onComplete;
            onConfirmed = null;
            itemNameText.text = item.Name.ToUpper();
            iconImage.sprite = item.GetIcon();
            priceUI.ShowPricesButtons(item.UnlockabilityInfo, ConfimrPurchase);
            OpenWindow(0);
            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="onComplete"></param>
        public void AskForConfirmation(ShopProductData item, Action<int> onComplete = null, string description = "")
        {
            if (itemDescriptionText != null)
            {
                if (string.IsNullOrEmpty(defaultDescription)) { defaultDescription = itemDescriptionText.text; }
                if (string.IsNullOrEmpty(description)) { description = defaultDescription; }
                itemDescriptionText.text = description;
            }

            Checkout(item);
            onConfirmed = onComplete;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator ProcessBuy(int coinID)
        {
#if ULSP && SHOP
            isBusy = true;
            bl_LobbyUI.Instance.overAllEmptyLoading.SetActive(true);
            var wf = bl_DataBaseUtils.CreateWWWForm(ULogin.FormHashParm.Name, true);
            wf.AddSecureField("type", 0);
            wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.ID);
            wf.AddSecureField("name", bl_DataBase.Instance.LocalUser.LoginName);
            //calculate the price with the coin conversion value.
            var priceForCoin = bl_MFPS.Coins.GetCoinData(coinID).DoConversion(ItemInCart.Price);
            wf.AddSecureField("coins", priceForCoin);
            wf.AddSecureField("coinid", coinID);

            //temp add the purchase
            List<bl_ShopPurchase> plist = bl_DataBase.LocalUserInstance.ShopData.ShopPurchases;
            var sp = new bl_ShopPurchase(ItemInCart);

            plist.Add(sp);
            wf.AddSecureField("line", bl_ShopData.CompilePurchases(plist));

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.GetUrl("shop"), wf))
            {
                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string result = w.downloadHandler.text;
                    if (result.Contains("done"))
                    {
                        bl_DataBase.Instance.LocalUser.ShopData.ShopPurchases = plist;
                        bl_DataBase.Instance.LocalUser.Coins[coinID] -= priceForCoin;
                        bl_EventHandler.DispatchCoinUpdate(null);
                        bl_ShopData.onItemPurchased?.Invoke();
                        OnPurchaseComplete();
                        Debug.Log("Purchase successfully");
                    }
                    else
                    {
                        Debug.LogWarning(result);
                    }
                }
                else if (w.responseCode == 404)
                {
                    Debug.LogWarning("Shop addon server script was not found, make sure you have upload it, check the Shop addon documentation.");
                }
                else
                {
                    Debug.LogError(w.error);
                }

                this.InvokeAfter(0.5f, () =>
                {
                    bl_LobbyUI.Instance.overAllEmptyLoading.SetActive(false);
                    isBusy = false;
                });
            }
#else
        yield break;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConfimrPurchase(int coinID)
        {
            if (isBusy || ItemInCart == null) return;
#if ULSP
            if (!bl_DataBase.IsUserLogged)
            {
                OpenWindow(2);
                Debug.Log("You have to log in with an account in order to make a purchase.");
                return;
            }

            // if the user does not have enough coins to buy the item
            if (!bl_UserWallet.HasFundsFor(ItemInCart.Price))
            {
                OpenWindow(1);
                return;
            }
#endif
            // if we are only asking for confirmation, that means we are going to handle the purchase in another way.
            if (onConfirmed != null)
            {
                onConfirmed.Invoke(coinID);
                onConfirmed = null;
                SetActive(false);
                return;
            }

            // if we are here, then we are going to process the purchase.
            StartCoroutine(ProcessBuy(coinID));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        private void OpenWindow(int id)
        {
            foreach (GameObject w in windows) w.SetActive(false);
            windows[id].SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OpenCoinWindow()
        {
            OpenWindow(0);
            bl_CoinsWindow.Instance?.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnPurchaseComplete()
        {
            onCompletePurchase?.Invoke();
            SetActive(false);
            bl_ShopAfterPurchaseWindow.Instance?.Show(ItemInCart);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateAccount()
        {
            bl_UtilityHelper.LoadLevel("Login");
        }

        public void SetActive(bool active) => content.SetActive(active);

        private static bl_CheckoutWindow _Instance;
        public static bl_CheckoutWindow Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_CheckoutWindow>();
                }
                return _Instance;
            }
        }
    }
}
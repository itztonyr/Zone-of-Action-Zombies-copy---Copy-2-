using MFPS.Internal.Structures;
using MFPS.Shop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class bl_ShopManager : MonoBehaviour
{
    #region Public members
    [Header("References")]
    public GameObject ContentUI;
    public GameObject ItemPrefab;
    public GameObject NoCoinsWindow;
    public Transform ListPanel;
    public TMP_Dropdown catDropDown;
    [SerializeField] private bl_ShopPreviewPanel previewPanel = null;
    public AnimationCurve ScaleCurve;
    #endregion

    #region Private members
    private readonly List<ShopProductData> Items = new List<ShopProductData>();
    List<bl_GunInfo> Weapons = new List<bl_GunInfo>();
    private readonly List<GameObject> cacheUI = new List<GameObject>();
    [HideInInspector] public bl_ShopData.ShopVirtualCoins coinPack;
    private ShopItemType sortByType = ShopItemType.None;
#if SHOP_UIAP
    private bl_UnityIAPShopHandler UnityIAPHandler;
#endif 
    public static Action<List<ShopProductData>> onBuildProducts;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        if (!bl_GameData.isDataCached)
        {
            while (!bl_GameData.isDataCached) { yield return null; }
        }
        sortByType = bl_ShopData.Instance.defaultShopCategory;
        if (Items == null || Items.Count <= 0) { BuildData(); }
        InstanceItems();
        SetUpUI();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
#if SHOP_UIAP
        UnityIAPHandler ??= new bl_UnityIAPShopHandler();
        bl_UnityIAP.Instance.InitializeIfNeeded();
        bl_ShopData.Instance.onPurchaseComplete += UnityIAPHandler.OnPurchaseResult;
#endif
        bl_ShopData.Instance.onPurchaseFailed += OnPurchaseFailed;
        bl_ShopData.onItemPurchased += OnItemPurchased;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
#if SHOP_UIAP
        bl_ShopData.Instance.onPurchaseComplete -= UnityIAPHandler.OnPurchaseResult;
#endif
        bl_ShopData.Instance.onPurchaseFailed -= OnPurchaseFailed;
        bl_ShopData.onItemPurchased -= OnItemPurchased;
    }

    /// <summary>
    /// 
    /// </summary>
    void BuildData()
    {
        Items.Clear();
        Weapons = bl_GameData.Instance.AllWeapons;
        for (int i = 0; i < Weapons.Count; i++)
        {
            var data = new ShopProductData
            {
                ID = i,
                Name = Weapons[i].Name,
                Type = ShopItemType.Weapon,
                GunInfo = Weapons[i],
                UnlockabilityInfo = Weapons[i].Unlockability
            };
            Items.Add(data);
        }

#if PSELECTOR
        var allPlayers = bl_PlayerSelector.Data.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            var pinfo = allPlayers[i];
            var data = new ShopProductData
            {
                ID = i,
                Name = pinfo.Name,
                Type = ShopItemType.PlayerSkin,
                PlayerSkinInfo = pinfo,
                UnlockabilityInfo = pinfo.Unlockability
            };
            Items.Add(data);
        }
#endif

#if CUSTOMIZER
        for (int i = 0; i < bl_CustomizerData.Instance.GlobalCamos.Count; i++)
        {
            if (i == 0) continue;//skip the default camo

            var gc = bl_CustomizerData.Instance.GlobalCamos[i];
            var data = new ShopProductData
            {
                ID = i,
                Name = gc.Name,
                Type = ShopItemType.WeaponCamo,
                UnlockabilityInfo = gc.Unlockability,
                camoInfo = gc
            };
            Items.Add(data);
        }
#endif

        // [START] SHOW EMBLEMS AND CALLING CARDS IN THE SHOP WINDOW
#if EACC
        if (bl_ShopData.Instance.showEmeblemsInShop)
        {
            for (int i = 0; i < bl_EmblemsDataBase.Instance.emblems.Count; i++)
            {
                var source = bl_EmblemsDataBase.Instance.emblems[i];
                var data = new ShopProductData
                {
                    ID = i,
                    Name = "Emblem",
                    Type = ShopItemType.Emblem,
                    UnlockabilityInfo = source.Unlockability
                };
                data.SetIcon(source.Emblem);
                Items.Add(data);
            }

            for (int i = 0; i < bl_EmblemsDataBase.Instance.callingCards.Count; i++)
            {
                var source = bl_EmblemsDataBase.Instance.callingCards[i];
                var data = new ShopProductData
                {
                    ID = i,
                    Name = source.Name,
                    Type = ShopItemType.CallingCard,
                    UnlockabilityInfo = source.Unlockability
                };
                data.SetIcon(source.Card);
                Items.Add(data);
            }
        }
#endif
        // [END] SHOW EMBLEMS AND CALLING CARDS IN THE SHOP WINDOW

        onBuildProducts?.Invoke(Items);
    }

    /// <summary>
    /// 
    /// </summary>
    public void InstanceItems()
    {
        InstanceItems(Items, bl_ShopData.Instance.randomizeItemsInShop);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    public void InstanceItems(List<ShopProductData> items, bool randomizeList = false)
    {
        CleanPanel();
        if (sortByType == ShopItemType.None && randomizeList)
        {
            Shuffle(items);
        }

        List<ShopProductData> nonOwnedItems = items.Where(x => x.IsUnlocked() == false).ToList();
        List<ShopProductData> ownedItems = items.Where(x => x.IsUnlocked()).ToList();

        // show the non-owned items at the top of the list
        for (int i = 0; i < nonOwnedItems.Count; i++)
        {
            var item = nonOwnedItems[i];

            if (sortByType != ShopItemType.None)
            {
                //sort items
                if (nonOwnedItems[i].Type != sortByType) continue;
            }

            InstanceItem(item, i == 0);
        }

        if (!bl_ShopData.Instance.showOwnedItems) return;

        for (int i = 0; i < ownedItems.Count; i++)
        {
            var item = ownedItems[i];

            if (sortByType != ShopItemType.None)
            {
                //sort items
                if (ownedItems[i].Type != sortByType) continue;
            }

            InstanceItem(item, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="preview"></param>
    void InstanceItem(ShopProductData item, bool preview)
    {
        // if the item is hidden, don't show it
        if (item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.Hidden || item.UnlockabilityInfo.IsVIPOnly()) return;
        // if the item is free or can't be purchased, don't show it (if the option is enabled)
        if ((item.Price <= 0 || !item.UnlockabilityInfo.CanBePurchased()) && !bl_ShopData.Instance.ShowFreeItems) return;
        // if the item can only be unlocked by level up and the option is enabled, don't show it
        if (item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly && !bl_ShopData.Instance.showBlockedByLevelOnly) return;

        GameObject g = Instantiate(ItemPrefab, ListPanel, false) as GameObject;
        g.SetActive(true);
        g.GetComponent<bl_ShopItemUIBase>().Setup(item);
        if (preview)
        {
            Preview(item, false, false);
        }
        cacheUI.Add(g);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowInventory()
    {
        CleanPanel();
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.UnlockabilityInfo.IsUnlocked(item.ID) || item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.Hidden) continue;

            GameObject g = Instantiate(ItemPrefab) as GameObject;
            g.SetActive(true);
            g.GetComponent<bl_ShopItemUIBase>().Setup(item);
            g.transform.SetParent(ListPanel, false);
            if (i == 0)
            {
                Preview(item, false, false);
            }
            cacheUI.Add(g);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PreviewItem(ShopProductData info, Vector3 origin)
    {
        bl_CheckoutWindow.Instance.Checkout(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void BuyCoinPack(int id)
    {
        coinPack = bl_ShopData.Instance.CoinsPacks[id];

        //add your payment system process here
        //use the 'coinPack' info like
        // coinPack.Price
        // coinPack.Name

        switch (bl_ShopData.Instance.ShopPayment)
        {
            case bl_ShopData.ShopPaymentTypes.UnityIAP:
                //Unity IAP integration here
                //check this: https://docs.unity3d.com/Manual/UnityIAP.html
#if SHOP_UIAP
                if (bl_UnityIAP.Instance == null)
                {
                    bl_LobbyUI.ShowOverAllMessage("Unity IAP addon has not been integrated.");
                    Debug.LogWarning("Unity IAP addon has not been integrated.");
                    return;
                }
                bl_UnityIAP.Instance.BuyProductID(coinPack.ID);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Paypal:
#if SHOP_PAYPAL && SHOP
                if (bl_Paypal.Instance == null)
                {
                    bl_LobbyUI.ShowOverAllMessage("Paypal addon has not been integrated.");
                    Debug.LogWarning("Paypal addon has not been integrated.");
                    return;
                }
                bl_Paypal.Instance.PurchaseCoinPack(coinPack);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Steam:
                //Steam IAP integration here
                //check this: https://partner.steamgames.com/doc/features/microtransactions
#if STEAM_MICROTXM
                if (bl_SteamPayment.Instance == null)
                {
                    Debug.LogWarning("Steam Payment addon has not been integrated.");
                    return;
                }

                bl_SteamPayment.Instance.PurchaseCoinPack(coinPack);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Other:
                //Your own payment API

                break;
            default:
                Debug.LogWarning("Payment not defined");
                break;
        }

        //once the payment is confirmed add the new coins to the player data using:
        //bl_DataBase.Instance.SaveNewCoins(coinPack.Amount);
        //that's

        bl_CoinsWindow.Instance.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="isOwned"></param>
    public void Preview(ShopProductData info, bool isOwned, bool canPurchase = true)
    {
        previewPanel?.Preview(info, isOwned, canPurchase);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeCategory(int categoryID)
    {
        if (categoryID == 0) { sortByType = ShopItemType.None; }
        else
        {
            categoryID--;
            sortByType = bl_ShopData.Instance.categorys[categoryID].itemType;
        }
        InstanceItems();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpUI()
    {
        catDropDown.ClearOptions();
        List<TMP_Dropdown.OptionData> cats = new List<TMP_Dropdown.OptionData>();
        List<ShopCategoryInfo> allcats = bl_ShopData.Instance.categorys;
        cats.Add(new TMP_Dropdown.OptionData() { text = "ALL" });
        for (int i = 0; i < allcats.Count; i++)
        {
            var od = new TMP_Dropdown.OptionData
            {
                text = allcats[i].Name.ToUpper()
            };
            cats.Add(od);
        }
        catDropDown.AddOptions(cats);
        ItemPrefab.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CleanPanel()
    {
        for (int i = 0; i < cacheUI.Count; i++)
        {
            if (cacheUI[i] == null) continue;
            Destroy(cacheUI[i]);
        }
        cacheUI.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenBuyCoinsWindow()
    {
        bl_CoinsWindow.Instance.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnItemPurchased()
    {
        InstanceItems();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPurchaseFailed()
    {

    }

    private readonly static System.Random rng = new System.Random();
    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    private static bl_ShopManager _instance = null;
    public static bl_ShopManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ShopManager>(); }
            if (_instance == null && bl_LobbyUI.Instance != null)
            {
                _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_ShopManager>(true);
            }
            return _instance;
        }
    }
}
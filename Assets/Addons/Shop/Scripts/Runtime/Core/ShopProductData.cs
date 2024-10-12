using MFPS.Internal.Structures;
using System;
using System.Collections.Generic;
using UnityEngine;
#if PSELECTOR
using MFPS.Addon.PlayerSelector;
#endif
#if CUSTOMIZER
#endif

namespace MFPS.Shop
{
    [Serializable]
    public class ShopProductData
    {
        public string Name;
        public ShopItemType Type = ShopItemType.Weapon;
        public int ID;
        public MFPSItemUnlockability UnlockabilityInfo;
        public bl_GunInfo GunInfo;
#if PSELECTOR
        public bl_PlayerSelectorInfo PlayerSkinInfo;
#endif
#if CUSTOMIZER
        public GlobalCamo camoInfo;
#endif
        private Sprite m_Icon = null;

        public int Price
        {
            get => UnlockabilityInfo.Price;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Sprite GetIcon()
        {
            if (m_Icon != null) return m_Icon;

            switch (Type)
            {
                case ShopItemType.Weapon:
                    return GunInfo.GunIcon;
                case ShopItemType.PlayerSkin:
#if PSELECTOR
                    return PlayerSkinInfo.Icon;
#else
                    return null;
#endif
                case ShopItemType.WeaponCamo:
#if CUSTOMIZER
                    return camoInfo.Icon;
#else
                    return null;
#endif
                default:
                    Debug.LogWarning($"Shop item '{Type.ToString()}' has not implemented the icon getter yet.");
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Texture2D icon)
        {
            SetIcon(Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Sprite icon) => m_Icon = icon;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDataBaseIdentifier()
        {
            return $"{(int)Type},{ID}-";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsFree()
        {
            return UnlockabilityInfo.IsFree();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUnlocked()
        {
            return UnlockabilityInfo.IsUnlocked(ID);
        }

        /// <summary>
        /// Return true only if this item can be purchased and is unlocked
        /// return false if is locked or is free
        /// </summary>
        public bool IsPurchased()
        {
            return UnlockabilityInfo.CanBePurchased() && IsUnlocked();
        }

        /// <summary>
        /// Collect all the MFPS purchasable items (weapons, player skins, and weapon camos)
        /// </summary>
        /// <returns></returns>
        public static List<ShopProductData> FetchAllInGamePurchasableItems(bool includeFreeItems = false)
        {
            var Items = new List<ShopProductData>();

            var Weapons = bl_GameData.Instance.AllWeapons;
            for (int i = 0; i < Weapons.Count; i++)
            {
                var weapon = Weapons[i];
                if (!includeFreeItems && weapon.Unlockability.IsFree())
                {
                    continue;
                }

                var data = new ShopProductData
                {
                    ID = i,
                    Name = weapon.Name,
                    Type = ShopItemType.Weapon,
                    GunInfo = weapon,
                    UnlockabilityInfo = weapon.Unlockability
                };
                Items.Add(data);
            }

#if PSELECTOR
            var allPlayers = bl_PlayerSelector.Data.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                var pinfo = allPlayers[i];
                if (!includeFreeItems && pinfo.Unlockability.IsFree())
                {
                    continue;
                }

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
                if (!includeFreeItems && gc.Unlockability.IsFree())
                {
                    continue;
                }

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

#if EACC
            var emblems = bl_EmblemsDataBase.Instance.emblems;
            for (int i = 0; i < emblems.Count; i++)
            {
                var emblem = emblems[i];
                if (!includeFreeItems && emblem.Unlockability.IsFree()) continue;

                var data = new ShopProductData
                {
                    ID = i,
                    Name = emblem.name,
                    Type = ShopItemType.Emblem,
                    UnlockabilityInfo = emblem.Unlockability
                };
                data.SetIcon(emblem.Emblem);
                Items.Add(data);
            }

            var ccards = bl_EmblemsDataBase.Instance.callingCards;
            for (int i = 0; i < ccards.Count; i++)
            {
                var card = ccards[i];
                if (!includeFreeItems && card.Unlockability.IsFree()) continue;

                var data = new ShopProductData
                {
                    ID = i,
                    Name = card.Name,
                    Type = ShopItemType.CallingCard,
                    UnlockabilityInfo = card.Unlockability
                };
                data.SetIcon(card.Card);
                Items.Add(data);
            }
#endif
            return Items;
        }
    }

    [Serializable]
    public class ShopCategoryInfo
    {
        public string Name;
        public ShopItemType itemType = ShopItemType.Weapon;
    }
}
using MFPS.Shop;
using System;

[Serializable]
public class bl_ShopPurchase
{
    public int TypeID = 0;
    public int ID = 0;

    public bl_ShopPurchase() { }

    public bl_ShopPurchase(int itemId, int itemType)
    {
        ID = itemId;
        TypeID = itemType;
    }

    public bl_ShopPurchase(ShopProductData item)
    {
        ID = item.ID;
        TypeID = (int)item.Type;
    }
}

[Serializable]
public enum ShopItemType
{
    Weapon = 0,
    WeaponCamo = 1,
    PlayerSkin = 2,
    PlayerAccesory = 3,
    Emblem = 4,
    CallingCard = 5,
    Emote = 6,
    SeasonPass = 7,
    LootBox = 8,
    Bundle = 9,
    CoinPack = 10,
    NoAds = 11,
    Coins = 12,
    VIP = 13,
    None = 99,
}
using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Shop
{
    [Serializable]
    public sealed class ShopGoods
    {
        public string goodsId;
        public string itemId;
        public CurrencyType currencyType;
        public int price;
        public int stock;
        public int requiredLevel;
    }

    [Serializable]
    public sealed class ShopSnapshot
    {
        public ShopGoods[] goldShop = Array.Empty<ShopGoods>();
        public ShopGoods[] honorShop = Array.Empty<ShopGoods>();
    }
}

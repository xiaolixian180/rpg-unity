using HeroQuest.Domain;

namespace HeroQuest.Systems.Shop
{
    public static class ShopRules
    {
        public static GameErrorCode ValidateBuy(ShopGoods goods, int buyerLevel, int count)
        {
            if (goods == null)
            {
                return GameErrorCode.ShopGoodsNotFound;
            }

            if (count <= 0)
            {
                return GameErrorCode.InvalidParameter;
            }

            if (goods.requiredLevel > buyerLevel)
            {
                return GameErrorCode.LevelNotEnough;
            }

            return goods.stock >= count ? GameErrorCode.Success : GameErrorCode.StockNotEnough;
        }
    }
}

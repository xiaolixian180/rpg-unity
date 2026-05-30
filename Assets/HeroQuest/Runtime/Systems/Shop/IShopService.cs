using HeroQuest.Domain;

namespace HeroQuest.Systems.Shop
{
    public interface IShopService
    {
        ServiceResult<ShopSnapshot> GetSnapshot();
        ServiceResult Buy(string goodsId, int count);
    }
}

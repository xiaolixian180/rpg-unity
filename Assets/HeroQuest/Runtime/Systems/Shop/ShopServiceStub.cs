using HeroQuest.Domain;

namespace HeroQuest.Systems.Shop
{
    public sealed class ShopServiceStub : IShopService
    {
        public ServiceResult<ShopSnapshot> GetSnapshot()
        {
            return ServiceResult<ShopSnapshot>.Success(new ShopSnapshot());
        }

        public ServiceResult Buy(string goodsId, int count)
        {
            return string.IsNullOrWhiteSpace(goodsId) || count <= 0
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter)
                : ServiceResult.Success();
        }
    }
}

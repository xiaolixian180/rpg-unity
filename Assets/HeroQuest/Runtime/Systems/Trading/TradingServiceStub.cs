using HeroQuest.Domain;

namespace HeroQuest.Systems.Trading
{
    public sealed class TradingServiceStub : ITradingService
    {
        public ServiceResult<TradeOrderPage> Browse(int page, int pageSize)
        {
            return ServiceResult<TradeOrderPage>.Success(new TradeOrderPage
            {
                page = page <= 0 ? 1 : page,
                pageSize = TradingRules.NormalizePageSize(pageSize, 20)
            });
        }

        public ServiceResult CreateOrder(string equipId, int price)
        {
            return string.IsNullOrWhiteSpace(equipId) || price <= 0
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter)
                : ServiceResult.Success();
        }

        public ServiceResult Buy(string orderId) => ValidateOrderId(orderId);
        public ServiceResult Cancel(string orderId) => ValidateOrderId(orderId);

        private static ServiceResult ValidateOrderId(string orderId)
        {
            return string.IsNullOrWhiteSpace(orderId)
                ? ServiceResult.Fail(GameErrorCode.TradeOrderNotFound)
                : ServiceResult.Success();
        }
    }
}

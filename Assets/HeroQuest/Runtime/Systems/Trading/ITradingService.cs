using HeroQuest.Domain;

namespace HeroQuest.Systems.Trading
{
    public interface ITradingService
    {
        ServiceResult<TradeOrderPage> Browse(int page, int pageSize);
        ServiceResult CreateOrder(string equipId, int price);
        ServiceResult Buy(string orderId);
        ServiceResult Cancel(string orderId);
    }
}

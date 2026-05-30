using HeroQuest.Domain;

namespace HeroQuest.Systems.Trading
{
    public static class TradingRules
    {
        public static bool CanBuy(string buyerId, TradeOrder order)
        {
            return order != null
                && order.status == TradeOrderStatus.Listed
                && !string.IsNullOrWhiteSpace(buyerId)
                && buyerId != order.sellerId;
        }

        public static bool CanCancel(string sellerId, TradeOrder order)
        {
            return order != null
                && order.status == TradeOrderStatus.Listed
                && !string.IsNullOrWhiteSpace(sellerId)
                && sellerId == order.sellerId;
        }

        public static int NormalizePageSize(int pageSize, int defaultPageSize)
        {
            return pageSize <= 0 ? defaultPageSize : pageSize;
        }
    }
}

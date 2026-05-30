using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Trading
{
    [Serializable]
    public sealed class TradeOrder
    {
        public string orderId;
        public string sellerId;
        public string equipId;
        public Quality quality;
        public int strengthenLevel;
        public int price;
        public TradeOrderStatus status;
    }

    [Serializable]
    public sealed class TradeOrderPage
    {
        public TradeOrder[] orders = Array.Empty<TradeOrder>();
        public int page;
        public int pageSize;
        public int total;
    }
}

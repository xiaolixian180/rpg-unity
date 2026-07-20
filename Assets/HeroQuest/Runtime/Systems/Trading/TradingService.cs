using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.Trading
{
    /// <summary>
    /// 交易行服务——真实网络实现。
    /// 通过 NetworkManager 发送 TradeList/TradePublish/TradeBuy/TradeCancel 请求，
    /// await 响应事件，映射为业务域模型。
    /// </summary>
    public sealed class TradingService : ITradingService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public TradingService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public async Task<ServiceResult<TradeOrderPage>> Browse(int category, int page, int pageSize)
        {
            int normalizedPage = page <= 0 ? 1 : page;
            int normalizedPageSize = TradingRules.NormalizePageSize(pageSize, 20);

            var result = await bridge.RequestAsync<GoTradeListResponse>(
                ct => networkManager.SendTradeListAsync(category, normalizedPage, normalizedPageSize, ct),
                h => networkManager.TradeListResult += h,
                h => networkManager.TradeListResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult<TradeOrderPage>.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult<TradeOrderPage>.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult<TradeOrderPage>.Success(new TradeOrderPage
            {
                orders = MapOrders(response.items),
                page = normalizedPage,
                pageSize = normalizedPageSize,
                total = response.total
            });
        }

        public async Task<ServiceResult> CreateOrder(int slot, int price)
        {
            if (slot < 0 || slot > 7)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidSlot);
            }

            if (price <= 0)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter);
            }

            var result = await bridge.RequestAsync<GoTradePublishResponse>(
                ct => networkManager.SendTradePublishAsync(slot, price, ct),
                h => networkManager.TradePublishResult += h,
                h => networkManager.TradePublishResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> Buy(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ServiceResult.Fail(GameErrorCode.TradeOrderNotFound);
            }

            if (!ulong.TryParse(orderId, out var orderIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid orderId: {orderId}");
            }

            var result = await bridge.RequestAsync<GoTradeBuyResponse>(
                ct => networkManager.SendTradeBuyAsync(orderIdValue, ct),
                h => networkManager.TradeBuyResult += h,
                h => networkManager.TradeBuyResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> Cancel(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ServiceResult.Fail(GameErrorCode.TradeOrderNotFound);
            }

            if (!ulong.TryParse(orderId, out var orderIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid orderId: {orderId}");
            }

            var result = await bridge.RequestAsync<GoTradeCancelResponse>(
                ct => networkManager.SendTradeCancelAsync(orderIdValue, ct),
                h => networkManager.TradeCancelResult += h,
                h => networkManager.TradeCancelResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }

        // ================================================================
        // 数据映射
        // ================================================================

        private static TradeOrder[] MapOrders(GoTradeItem[] items)
        {
            if (items == null || items.Length == 0)
            {
                return Array.Empty<TradeOrder>();
            }

            var orders = new TradeOrder[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                orders[i] = new TradeOrder
                {
                    orderId = item.order_id.ToString(),
                    sellerId = item.seller_id.ToString(),
                    equipId = item.equip_id.ToString(),
                    quality = (Quality)item.quality,
                    strengthenLevel = item.strengthen_level,
                    price = (int)item.price,
                    status = TradeOrderStatus.Listed
                };
            }

            return orders;
        }
    }
}

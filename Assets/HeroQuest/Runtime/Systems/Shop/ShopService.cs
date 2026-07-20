using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.Shop
{
    /// <summary>
    /// 商店服务——真实网络实现。
    /// 通过 NetworkManager 发送 ShopList/ShopBuy 请求，await 响应事件，映射为业务域模型。
    /// GetSnapshot 依次拉取金币商店(type=0)和荣誉商店(type=1)，合并返回。
    /// </summary>
    public sealed class ShopService : IShopService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public ShopService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public async Task<ServiceResult<ShopSnapshot>> GetSnapshot()
        {
            // 拉取金币商店（type=0）
            var goldResult = await FetchShopList(0);
            if (!goldResult.IsSuccess)
            {
                return ServiceResult<ShopSnapshot>.Fail(goldResult.ErrorCode, goldResult.Message);
            }

            // 拉取荣誉商店（type=1）
            var honorResult = await FetchShopList(1);
            if (!honorResult.IsSuccess)
            {
                return ServiceResult<ShopSnapshot>.Fail(honorResult.ErrorCode, honorResult.Message);
            }

            return ServiceResult<ShopSnapshot>.Success(new ShopSnapshot
            {
                goldShop = goldResult.Value,
                honorShop = honorResult.Value
            });
        }

        public async Task<ServiceResult> Buy(string goodsId, int count, CurrencyType currencyType)
        {
            if (string.IsNullOrWhiteSpace(goodsId) || count <= 0)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter);
            }

            if (!ulong.TryParse(goodsId, out var itemId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid goodsId: {goodsId}");
            }

            var result = await bridge.RequestAsync<GoShopBuyResponse>(
                ct => networkManager.SendShopBuyAsync(itemId, count, (int)currencyType, ct),
                h => networkManager.ShopBuyResult += h,
                h => networkManager.ShopBuyResult -= h);

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
        // 内部方法
        // ================================================================

        private async Task<ServiceResult<ShopGoods[]>> FetchShopList(int shopType)
        {
            var result = await bridge.RequestAsync<GoShopListResponse>(
                ct => networkManager.SendShopListAsync(shopType, ct),
                h => networkManager.ShopListResult += h,
                h => networkManager.ShopListResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult<ShopGoods[]>.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult<ShopGoods[]>.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult<ShopGoods[]>.Success(MapShopGoods(response.items));
        }

        private static ShopGoods[] MapShopGoods(GoShopItem[] items)
        {
            if (items == null || items.Length == 0)
            {
                return Array.Empty<ShopGoods>();
            }

            var goods = new ShopGoods[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                goods[i] = new ShopGoods
                {
                    goodsId = item.id.ToString(),
                    itemId = item.id.ToString(),
                    currencyType = (CurrencyType)item.currency_type,
                    price = (int)item.price,
                    stock = item.stock,
                    requiredLevel = item.require_level
                };
            }

            return goods;
        }
    }
}

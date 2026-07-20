using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;

namespace HeroQuest.Systems.Inventory
{
    /// <summary>
    /// 背包服务——真实网络实现。
    /// 使用物品通过 NetworkManager 发送 UseItem 请求；背包数据随 InventorySync 推送同步。
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;
        private InventorySnapshot cachedSnapshot;

        public InventoryService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
            cachedSnapshot = new InventorySnapshot();

            networkManager.InventorySync += OnInventorySync;
        }

        public Task<ServiceResult<InventorySnapshot>> GetSnapshot()
        {
            return Task.FromResult(ServiceResult<InventorySnapshot>.Success(cachedSnapshot));
        }

        public Task<ServiceResult> AddItem(string itemId, ItemCategory category, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
            {
                return Task.FromResult(ServiceResult.Fail(GameErrorCode.InvalidParameter));
            }

            // 无"添加物品"协议：物品由服务端自动入包
            return Task.FromResult(ServiceResult.Success());
        }

        public async Task<ServiceResult> ConsumeItem(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId) || count <= 0)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter);
            }

            if (!uint.TryParse(itemId, out var id))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid itemId: {itemId}");
            }

            var result = await bridge.RequestAsync<GoUseItemResponse>(
                ct => networkManager.SendUseItemAsync(id, ct),
                h => networkManager.UseItemResult += h,
                h => networkManager.UseItemResult -= h);

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

        private void OnInventorySync(GoInventorySync sync)
        {
            if (sync?.items == null) return;

            var stacks = new InventoryItemStack[sync.items.Length];
            for (int i = 0; i < sync.items.Length; i++)
            {
                var item = sync.items[i];
                stacks[i] = new InventoryItemStack
                {
                    itemId = item.item_id.ToString(),
                    category = ItemCategory.Consumable,
                    count = item.count
                };
            }

            cachedSnapshot = new InventorySnapshot { items = stacks };
        }
    }
}

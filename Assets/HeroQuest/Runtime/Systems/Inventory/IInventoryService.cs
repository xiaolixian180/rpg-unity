using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Inventory
{
    /// <summary>
    /// 背包服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// 获取背包快照。
        /// 背包数据随 GoPlayerData.items / InventorySync 推送同步，无独立查询协议——本地返回空快照。
        /// </summary>
        Task<ServiceResult<InventorySnapshot>> GetSnapshot();

        /// <summary>
        /// 添加物品。
        /// 无"添加物品"协议：物品由服务端在掉落/购买/奖励等事件时自动入包，本地仅做参数校验。
        /// </summary>
        /// <param name="itemId">物品 ID（字符串形式的 uint，对应协议 item_id）</param>
        /// <param name="category">物品类别</param>
        /// <param name="count">数量</param>
        Task<ServiceResult> AddItem(string itemId, ItemCategory category, int count);

        /// <summary>
        /// 消耗（使用）物品。
        /// 对应协议 UseItem（仅以 item_id 为单位，服务端按单次使用扣减）。
        /// </summary>
        /// <param name="itemId">物品 ID（字符串形式的 uint，对应协议 item_id）</param>
        /// <param name="count">数量（校验用，协议不支持批量使用）</param>
        Task<ServiceResult> ConsumeItem(string itemId, int count);
    }
}

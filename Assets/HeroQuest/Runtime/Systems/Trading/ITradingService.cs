using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Trading
{
    /// <summary>
    /// 交易行服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface ITradingService
    {
        /// <summary>
        /// 浏览交易行商品列表。
        /// </summary>
        /// <param name="category">物品分类（0=全部 1=武器 2=防具 ...，具体由服务端定义）</param>
        /// <param name="page">页码（从 1 开始）</param>
        /// <param name="pageSize">期望每页条数（客户端展示用，服务端分页大小固定）</param>
        Task<ServiceResult<TradeOrderPage>> Browse(int category, int page, int pageSize);

        /// <summary>
        /// 上架装备到交易行。
        /// </summary>
        /// <param name="slot">装备槽位（0~7，对应 EquipmentSlot 枚举值）</param>
        /// <param name="price">售价（金币）</param>
        Task<ServiceResult> CreateOrder(int slot, int price);

        /// <summary>
        /// 购买交易行商品。
        /// </summary>
        /// <param name="orderId">订单 ID（字符串形式的 ulong）</param>
        Task<ServiceResult> Buy(string orderId);

        /// <summary>
        /// 取消上架。
        /// </summary>
        /// <param name="orderId">订单 ID（字符串形式的 ulong）</param>
        Task<ServiceResult> Cancel(string orderId);
    }
}

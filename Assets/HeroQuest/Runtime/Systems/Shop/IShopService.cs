using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Shop
{
    /// <summary>
    /// 商店服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IShopService
    {
        /// <summary>
        /// 获取商店快照（金币商店 + 荣誉商店）。
        /// 内部依次拉取 type=0（金币）和 type=1（荣誉）两类商品列表。
        /// </summary>
        Task<ServiceResult<ShopSnapshot>> GetSnapshot();

        /// <summary>
        /// 购买商品。
        /// </summary>
        /// <param name="goodsId">商品 ID（对应协议 item_id，字符串形式的 ulong）</param>
        /// <param name="count">购买数量</param>
        /// <param name="currencyType">货币类型（Gold=0 / Honor=1）</param>
        Task<ServiceResult> Buy(string goodsId, int count, CurrencyType currencyType);
    }
}

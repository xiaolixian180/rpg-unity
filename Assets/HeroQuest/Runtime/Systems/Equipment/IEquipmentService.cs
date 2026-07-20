using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Equipment
{
    /// <summary>
    /// 装备服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>
        /// 获取装备快照（已穿戴的装备列表）。
        /// 装备数据随 GoPlayerData.equipment 同步下发，无独立查询协议——本地返回空快照。
        /// </summary>
        Task<ServiceResult<EquipmentSnapshot>> GetSnapshot();

        /// <summary>
        /// 穿戴装备。
        /// </summary>
        /// <param name="equipId">装备实例 ID（字符串形式的 ulong，对应协议 equip_id）</param>
        /// <param name="slot">目标装备槽位</param>
        Task<ServiceResult> Equip(string equipId, EquipmentSlot slot);

        /// <summary>
        /// 卸下装备。
        /// </summary>
        /// <param name="slot">装备槽位</param>
        Task<ServiceResult> Unequip(EquipmentSlot slot);

        /// <summary>
        /// 强化装备。
        /// </summary>
        /// <param name="equipId">装备槽位编号（字符串形式的 int，对应协议 slot；强化以槽位为单位）</param>
        Task<ServiceResult> Strengthen(string equipId);

        /// <summary>
        /// 附魔装备。
        /// </summary>
        /// <param name="equipId">装备槽位编号（字符串形式的 int，对应协议 slot）</param>
        /// <param name="attributeId">附魔材料 ID（字符串形式的 ulong，对应协议 material_id）</param>
        Task<ServiceResult> Enchant(string equipId, string attributeId);
    }
}

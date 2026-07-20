using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Pets
{
    /// <summary>
    /// 宠物服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IPetService
    {
        /// <summary>
        /// 获取宠物快照。
        /// 宠物数据无独立列表查询协议（随各宠物操作响应下发），本地返回空快照。
        /// </summary>
        Task<ServiceResult<PetSnapshot>> GetSnapshot();

        /// <summary>
        /// 召唤宠物（出战）。
        /// </summary>
        /// <param name="petId">宠物实例 UID（字符串形式的 ulong，对应协议 pet_uid）</param>
        Task<ServiceResult> Summon(string petId);

        /// <summary>
        /// 收回宠物（休息）。
        /// </summary>
        /// <param name="petId">宠物实例 UID（字符串形式的 ulong，对应协议 pet_uid）</param>
        Task<ServiceResult> Retract(string petId);

        /// <summary>
        /// 宠物升级。
        /// </summary>
        /// <param name="petId">宠物实例 UID（字符串形式的 ulong，对应协议 pet_uid）</param>
        Task<ServiceResult> LevelUp(string petId);

        /// <summary>
        /// 开始探索。
        /// </summary>
        /// <param name="petId">宠物实例 UID（字符串形式的 ulong，对应协议 pet_uid）</param>
        /// <param name="duration">探索时长（秒，对应协议 duration）</param>
        Task<ServiceResult> StartExplore(string petId, int duration);

        /// <summary>
        /// 结束探索。
        /// 无"结束探索"协议：探索由服务端在 duration 到期后自动完成并推送奖励，本地仅做参数校验。
        /// </summary>
        /// <param name="petId">宠物实例 UID（字符串形式的 ulong，对应协议 pet_uid）</param>
        Task<ServiceResult> FinishExplore(string petId);

        /// <summary>
        /// 合成宠物（消耗材料宠物）。
        /// </summary>
        /// <param name="materialPetIds">材料宠物 UID 数组（长度须为 PetRules.SynthesizeMaterialCount）</param>
        Task<ServiceResult> Synthesize(string[] materialPetIds);
    }
}

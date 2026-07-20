using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Skills
{
    /// <summary>
    /// 技能服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// 获取技能快照（已学技能列表 + 剩余技能点）。
        /// 无独立查询协议——技能数据由服务端在升级/重置响应中隐式同步，本地返回空快照。
        /// </summary>
        Task<ServiceResult<SkillSnapshot>> GetSnapshot();

        /// <summary>
        /// 学习或升级技能。
        /// </summary>
        /// <param name="skillId">技能 ID（字符串形式的 int，对应协议 skill_id）</param>
        Task<ServiceResult> LearnOrUpgrade(string skillId);

        /// <summary>
        /// 释放技能。
        /// 技能效果为广播型消息（SkillEffectReceived），由战斗层另行订阅处理。
        /// </summary>
        /// <param name="skillId">技能 ID（字符串形式的 int）</param>
        /// <param name="targetId">目标 ID（字符串形式的 ulong）</param>
        Task<ServiceResult> Cast(string skillId, string targetId);

        /// <summary>
        /// 重置全部技能。
        /// </summary>
        /// <param name="goldCost">金币消耗（仅做客户端校验，服务端按规则扣除）</param>
        Task<ServiceResult> ResetAll(int goldCost);
    }
}

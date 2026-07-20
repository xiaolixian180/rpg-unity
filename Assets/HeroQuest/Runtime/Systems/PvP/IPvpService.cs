using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.PvP
{
    /// <summary>
    /// PvP 服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IPvpService
    {
        /// <summary>
        /// 预览击杀奖励（纯客户端计算，基于 PvpRules，无服务端往返）。
        /// 实际奖励以 <see cref="Attack"/> 响应（GoPvpResult）为准。
        /// </summary>
        /// <param name="targetGold">目标持有金币</param>
        /// <param name="targetKillValue">目标杀戮值</param>
        Task<ServiceResult<PvpRewardPreview>> PreviewKillReward(int targetGold, int targetKillValue);

        /// <summary>
        /// 攻击目标玩家。
        /// </summary>
        /// <param name="request">攻击请求（targetId/skillId；attackerId 由服务端推断，本地忽略）</param>
        Task<ServiceResult> Attack(PvpAttackRequest request);

        /// <summary>
        /// 领取悬赏奖励（悬赏目标）。
        /// </summary>
        /// <param name="targetPlayerId">目标玩家 ID（字符串形式的 ulong）</param>
        Task<ServiceResult> ClaimBounty(string targetPlayerId);

        /// <summary>
        /// 复仇。
        /// </summary>
        /// <param name="targetPlayerId">目标玩家 ID（字符串形式的 ulong）</param>
        Task<ServiceResult> Revenge(string targetPlayerId);
    }
}

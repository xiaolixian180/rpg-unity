using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Ranking
{
    /// <summary>
    /// 排行榜服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IRankingService
    {
        /// <summary>
        /// 查询指定类型的排行榜数据。
        /// 通过网络请求获取（协议: RankingList 2101/2102）。
        /// </summary>
        /// <param name="rankingType">排行榜类型（Level/Power/Honor）</param>
        /// <param name="limit">期望返回的条目数上限（服务端固定返回数量，此参数仅做客户端校验）</param>
        Task<ServiceResult<RankingSnapshot>> GetTop(RankingType rankingType, int limit = 50);

        /// <summary>
        /// 上报分数。
        /// 无服务端协议支持——排行榜分数由服务端在击杀/Boss死亡等事件时自动上报。
        /// 本地 no-op，仅做参数校验。
        /// </summary>
        Task<ServiceResult> ReportScore(RankingType rankingType, long score);
    }
}

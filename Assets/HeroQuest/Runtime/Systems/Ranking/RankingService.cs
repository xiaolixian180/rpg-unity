using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.Ranking
{
    /// <summary>
    /// 排行榜服务——真实网络实现。
    /// 通过 NetworkManager 发送 RankingList 请求，await 响应事件，映射为业务域模型。
    /// ReportScore 无服务端协议支持，本地 no-op。
    /// </summary>
    public sealed class RankingService : IRankingService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public RankingService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public async Task<ServiceResult<RankingSnapshot>> GetTop(RankingType rankingType, int limit = 50)
        {
            if (limit <= 0)
            {
                return ServiceResult<RankingSnapshot>.Fail(GameErrorCode.InvalidParameter);
            }

            var result = await bridge.RequestAsync<GoRankingListResponse>(
                ct => networkManager.SendRankingListAsync((int)rankingType, limit, ct),
                h => networkManager.RankingListResult += h,
                h => networkManager.RankingListResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult<RankingSnapshot>.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult<RankingSnapshot>.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult<RankingSnapshot>.Success(MapSnapshot(response, rankingType));
        }

        public Task<ServiceResult> ReportScore(RankingType rankingType, long score)
        {
            // 无服务端协议支持：排行榜分数由服务端在击杀/Boss死亡等事件时自动上报。
            // 本地 no-op，仅做参数校验。
            if (score < 0)
            {
                return Task.FromResult(ServiceResult.Fail(GameErrorCode.InvalidParameter));
            }

            return Task.FromResult(ServiceResult.Success());
        }

        // ================================================================
        // 数据映射
        // ================================================================

        private static RankingSnapshot MapSnapshot(GoRankingListResponse response, RankingType rankingType)
        {
            var rankings = response.rankings;
            if (rankings == null || rankings.Length == 0)
            {
                return new RankingSnapshot();
            }

            var entries = new RankingEntry[rankings.Length];
            for (int i = 0; i < rankings.Length; i++)
            {
                var item = rankings[i];
                entries[i] = new RankingEntry
                {
                    playerId = item.player_id.ToString(),
                    displayName = item.name,
                    rankingType = rankingType,
                    score = item.value,
                    rank = item.rank
                };
            }

            return new RankingSnapshot { entries = entries };
        }
    }
}

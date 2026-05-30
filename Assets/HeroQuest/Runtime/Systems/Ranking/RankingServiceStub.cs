using HeroQuest.Domain;

namespace HeroQuest.Systems.Ranking
{
    public sealed class RankingServiceStub : IRankingService
    {
        public ServiceResult<RankingSnapshot> GetTop(RankingType rankingType, int limit = 50)
        {
            return limit <= 0
                ? ServiceResult<RankingSnapshot>.Fail(GameErrorCode.InvalidParameter)
                : ServiceResult<RankingSnapshot>.Success(new RankingSnapshot());
        }

        public ServiceResult ReportScore(RankingType rankingType, long score)
        {
            return score < 0
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter)
                : ServiceResult.Success();
        }
    }
}

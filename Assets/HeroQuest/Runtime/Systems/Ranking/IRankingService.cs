using HeroQuest.Domain;

namespace HeroQuest.Systems.Ranking
{
    public interface IRankingService
    {
        ServiceResult<RankingSnapshot> GetTop(RankingType rankingType, int limit = 50);
        ServiceResult ReportScore(RankingType rankingType, long score);
    }
}

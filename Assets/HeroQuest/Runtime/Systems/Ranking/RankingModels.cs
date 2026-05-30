using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Ranking
{
    [Serializable]
    public sealed class RankingEntry
    {
        public string playerId;
        public string displayName;
        public RankingType rankingType;
        public long score;
        public int rank;
    }

    [Serializable]
    public sealed class RankingSnapshot
    {
        public RankingEntry[] entries = Array.Empty<RankingEntry>();
    }
}

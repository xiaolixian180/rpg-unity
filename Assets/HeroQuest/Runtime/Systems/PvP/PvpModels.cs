using System;

namespace HeroQuest.Systems.PvP
{
    [Serializable]
    public sealed class PvpAttackRequest
    {
        public string attackerId;
        public string targetId;
        public string skillId;
    }

    [Serializable]
    public sealed class PvpRewardPreview
    {
        public int goldReward;
        public int honorReward;
        public int killValueDelta;
        public bool targetBecomesRedName;
    }
}

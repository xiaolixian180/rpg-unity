using System;
using HeroQuest.Domain;

namespace HeroQuest.Player
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public string playerId;
        public string displayName;
        public CharacterClass characterClass;
        public int level = 1;
        public int experience;
        public int gold;
        public int maxUnlockedLayer = 1;
        public StatBlock baseStats = new(5, 5, 5, 5, 0);
    }
}

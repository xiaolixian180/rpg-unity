using System;

namespace HeroQuest.Domain
{
    [Serializable]
    public readonly struct CombatStats
    {
        public CombatStats(int maxHp, float attack, float defense, float dodgeRate, float criticalRate, float criticalMultiplier)
        {
            MaxHp = maxHp;
            Attack = attack;
            Defense = defense;
            DodgeRate = dodgeRate;
            CriticalRate = criticalRate;
            CriticalMultiplier = criticalMultiplier;
        }

        public int MaxHp { get; }
        public float Attack { get; }
        public float Defense { get; }
        public float DodgeRate { get; }
        public float CriticalRate { get; }
        public float CriticalMultiplier { get; }
    }
}

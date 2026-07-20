using UnityEngine;

namespace HeroQuest.Domain
{
    public static class CombatCalculator
    {
        private const float DefaultMaxDodgeRate = 0.30f;
        private const float DefaultMaxCriticalRate = 0.50f;

        public static CombatStats CalculateStats(int level, StatBlock stats)
        {
            return CalculateStats(level, stats, DefaultMaxDodgeRate, DefaultMaxCriticalRate);
        }

        public static CombatStats CalculateStats(int level, StatBlock stats, float maxDodgeRate, float maxCriticalRate)
        {
            var clampedLevel = Mathf.Max(1, level);
            var maxHp = (100 + clampedLevel * 20) + stats.constitution * 10 + stats.strength * 5 + stats.defense * 3;
            var attack = stats.strength * 2f + clampedLevel * 5f + stats.agility * 0.5f;
            var defense = stats.defense * 3f + stats.constitution + clampedLevel * 2f;
            var dodgeRate = Mathf.Min(stats.agility * 0.005f, maxDodgeRate);
            var criticalRate = Mathf.Min(stats.agility * 0.003f + stats.strength * 0.001f, maxCriticalRate);
            var criticalMultiplier = 1.5f + stats.strength * 0.01f;

            return new CombatStats(maxHp, attack, defense, dodgeRate, criticalRate, criticalMultiplier);
        }

        public static int CalculateDamage(float attack, float skillMultiplier, float targetDefense, bool isCritical, float criticalMultiplier)
        {
            var damage = Mathf.Max(0f, attack) * Mathf.Max(0f, skillMultiplier);
            if (isCritical)
            {
                damage *= Mathf.Max(1f, criticalMultiplier);
            }

            return Mathf.Max(1, Mathf.FloorToInt(damage - Mathf.Max(0f, targetDefense)));
        }
    }
}

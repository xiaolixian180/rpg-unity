using UnityEngine;

namespace HeroQuest.Systems.Skills
{
    public static class SkillRules
    {
        public const float DefaultDamageMultiplier = 1.5f;

        public static bool CanUpgrade(int currentLevel, int maxLevel, int availablePoints)
        {
            return currentLevel >= 0 && currentLevel < maxLevel && availablePoints > 0;
        }

        public static float ResolveDamageMultiplier(float configuredMultiplier)
        {
            return configuredMultiplier > 0f ? configuredMultiplier : DefaultDamageMultiplier;
        }

        public static bool IsCooldownReady(float cooldownRemaining)
        {
            return Mathf.Approximately(cooldownRemaining, 0f) || cooldownRemaining < 0f;
        }
    }
}

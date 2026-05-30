using HeroQuest.Domain;
using UnityEngine;

namespace HeroQuest.Systems.Equipment
{
    public static class EquipmentRules
    {
        public static bool IsValidStrengthenLevel(int level)
        {
            return level >= 0 && level <= 15;
        }

        public static int GetStrengthenGoldCost(int nextLevel)
        {
            return Mathf.Max(1, nextLevel) * 100;
        }

        public static float GetQualityPowerMultiplier(Quality quality)
        {
            return 1f + (int)quality * 0.25f;
        }
    }
}

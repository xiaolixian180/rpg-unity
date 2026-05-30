using UnityEngine;

namespace HeroQuest.Systems.PvP
{
    public static class PvpRules
    {
        public static bool IsRedName(int killValue, int threshold)
        {
            return killValue >= Mathf.Max(1, threshold);
        }

        public static int CalculateRobbedGold(int targetGold, float robberyRate)
        {
            return Mathf.Max(0, Mathf.FloorToInt(targetGold * Mathf.Clamp01(robberyRate)));
        }

        public static int CalculateBountyGold(int targetKillValue, int goldPerKillValue)
        {
            return Mathf.Max(0, targetKillValue) * Mathf.Max(0, goldPerKillValue);
        }
    }
}

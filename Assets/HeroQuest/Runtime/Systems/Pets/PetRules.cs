using HeroQuest.Domain;

namespace HeroQuest.Systems.Pets
{
    public static class PetRules
    {
        public const int SynthesizeMaterialCount = 3;

        public static bool CanSynthesize(Quality firstQuality, Quality secondQuality, Quality thirdQuality)
        {
            return firstQuality == secondQuality && secondQuality == thirdQuality;
        }

        public static int GetLevelUpGoldCost(int currentLevel, int baseCost)
        {
            return currentLevel <= 1 ? baseCost : baseCost * currentLevel;
        }
    }
}

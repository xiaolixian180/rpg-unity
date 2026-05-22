using UnityEngine;

namespace HeroQuest.Systems.Dungeon
{
    public static class DungeonRules
    {
        public static bool IsBossLayer(int layer)
        {
            return layer > 0 && layer % 10 == 0;
        }

        public static bool CanEnterLayer(int targetLayer, int maxUnlockedLayer, int configuredMaxLayer)
        {
            if (targetLayer < 1 || targetLayer > Mathf.Max(1, configuredMaxLayer))
            {
                return false;
            }

            return targetLayer <= Mathf.Max(1, maxUnlockedLayer) + 1;
        }

        public static MonsterTemplate GenerateFallbackMonster(int layer)
        {
            var clampedLayer = Mathf.Max(1, layer);
            return new MonsterTemplate(
                clampedLayer,
                $"Layer {clampedLayer} Monster",
                50 + clampedLayer * 30,
                5 + clampedLayer * 3,
                2 + clampedLayer * 2,
                Mathf.Max(1, clampedLayer * 10),
                Mathf.Max(1, clampedLayer * 5));
        }
    }
}

using UnityEngine;

namespace HeroQuest.Config
{
    [CreateAssetMenu(menuName = "Hero Quest/Game Balance", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Progression")]
        [Min(1)] public int maxLevel = 60;
        [Min(1)] public int maxDungeonLayer = 30;
        [Min(0)] public int attributePointsPerLevel = 3;

        [Header("Dungeon")]
        [Min(0)] public int teleportGoldCost = 100;
        [Min(0)] public float monsterRespawnSeconds = 30f;

        [Header("Combat Caps")]
        [Range(0f, 1f)] public float maxDodgeRate = 0.30f;
        [Range(0f, 1f)] public float maxCriticalRate = 0.50f;
    }
}

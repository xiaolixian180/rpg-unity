using UnityEngine;

namespace HeroQuest.Config
{
    [CreateAssetMenu(menuName = "Hero Quest/Product Rule Config", fileName = "ProductRuleConfig")]
    public sealed class ProductRuleConfig : ScriptableObject
    {
        [Header("Progression")]
        [Min(1)] public int maxLevel = 60;
        [Min(1)] public int maxDungeonLayer = 30;
        [Min(1)] public int defaultPageSize = 20;

        [Header("Economy")]
        [Min(0)] public int skillResetGoldCost = 1000;
        [Min(0)] public int dungeonTeleportGoldCost = 100;
        [Min(0)] public int petLevelUpGoldCost = 100;

        [Header("PvP")]
        [Min(0)] public int redNameKillValue = 5;
        [Min(0)] public int invincibleSecondsAfterDeath = 30;
        [Range(0f, 1f)] public float pvpGoldRobberyRate = 0.10f;
        [Min(0)] public int pvpHonorReward = 10;
        [Min(0)] public int bountyGoldPerKillValue = 500;
        [Min(0)] public int bountyHonorReward = 10;

        [Header("Boss")]
        [Range(0f, 1f)] public float bossEpicDropRate = 0.10f;
        [Range(0f, 1f)] public float bossLegendaryDropRate = 0.02f;

        [Header("Server")]
        [Min(1)] public int autosaveIntervalSeconds = 60;
        [Min(1)] public int requestLimitPerSecond = 30;
        [Min(1)] public int maxMessageBytes = 4096;
        [Min(1)] public int maxConnectionsPerIp = 5;
    }
}

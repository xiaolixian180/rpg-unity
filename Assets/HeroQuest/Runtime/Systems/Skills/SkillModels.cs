using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Skills
{
    [Serializable]
    public sealed class SkillDefinition
    {
        public string skillId;
        public CharacterClass ownerClass;
        public SkillKind kind;
        public int maxLevel = 10;
        public float damageMultiplier = 1.5f;
        public float cooldownSeconds = 1f;
    }

    [Serializable]
    public sealed class PlayerSkill
    {
        public string skillId;
        public int level;
        public float cooldownRemaining;
    }

    [Serializable]
    public sealed class SkillSnapshot
    {
        public PlayerSkill[] skills = Array.Empty<PlayerSkill>();
        public int skillPoints;
    }
}

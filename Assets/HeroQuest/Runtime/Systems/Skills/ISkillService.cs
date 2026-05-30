using HeroQuest.Domain;

namespace HeroQuest.Systems.Skills
{
    public interface ISkillService
    {
        ServiceResult<SkillSnapshot> GetSnapshot();
        ServiceResult LearnOrUpgrade(string skillId);
        ServiceResult Cast(string skillId, string targetId);
        ServiceResult ResetAll(int goldCost);
    }
}

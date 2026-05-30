using HeroQuest.Domain;

namespace HeroQuest.Systems.Skills
{
    public sealed class SkillServiceStub : ISkillService
    {
        public ServiceResult<SkillSnapshot> GetSnapshot()
        {
            return ServiceResult<SkillSnapshot>.Success(new SkillSnapshot());
        }

        public ServiceResult LearnOrUpgrade(string skillId)
        {
            return string.IsNullOrWhiteSpace(skillId)
                ? ServiceResult.Fail(GameErrorCode.SkillNotFound, "Skill id is required.")
                : ServiceResult.Success();
        }

        public ServiceResult Cast(string skillId, string targetId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return ServiceResult.Fail(GameErrorCode.SkillNotFound, "Skill id is required.");
            }

            return string.IsNullOrWhiteSpace(targetId)
                ? ServiceResult.Fail(GameErrorCode.TargetNotFound, "Target id is required.")
                : ServiceResult.Success();
        }

        public ServiceResult ResetAll(int goldCost)
        {
            return goldCost < 0
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter, "Gold cost cannot be negative.")
                : ServiceResult.Success();
        }
    }
}

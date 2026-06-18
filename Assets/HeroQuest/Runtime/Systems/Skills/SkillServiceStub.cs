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
                ? ServiceResult.Fail(GameErrorCode.SkillNotFound, "技能编号不能为空。")
                : ServiceResult.Success();
        }

        public ServiceResult Cast(string skillId, string targetId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return ServiceResult.Fail(GameErrorCode.SkillNotFound, "技能编号不能为空。");
            }

            return string.IsNullOrWhiteSpace(targetId)
                ? ServiceResult.Fail(GameErrorCode.TargetNotFound, "目标编号不能为空。")
                : ServiceResult.Success();
        }

        public ServiceResult ResetAll(int goldCost)
        {
            return goldCost < 0
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter, "金币消耗不能为负数。")
                : ServiceResult.Success();
        }
    }
}

using HeroQuest.Domain;

namespace HeroQuest.Systems.PvP
{
    public sealed class PvpServiceStub : IPvpService
    {
        public ServiceResult<PvpRewardPreview> PreviewKillReward(int targetGold, int targetKillValue)
        {
            return ServiceResult<PvpRewardPreview>.Success(new PvpRewardPreview
            {
                goldReward = PvpRules.CalculateRobbedGold(targetGold, 0.1f),
                honorReward = 10,
                killValueDelta = 1,
                targetBecomesRedName = PvpRules.IsRedName(targetKillValue, 5)
            });
        }

        public ServiceResult Attack(PvpAttackRequest request)
        {
            return request == null || string.IsNullOrWhiteSpace(request.targetId)
                ? ServiceResult.Fail(GameErrorCode.TargetNotFound)
                : ServiceResult.Success();
        }

        public ServiceResult ClaimBounty(string targetPlayerId) => ValidateTarget(targetPlayerId);
        public ServiceResult Revenge(string targetPlayerId) => ValidateTarget(targetPlayerId);

        private static ServiceResult ValidateTarget(string targetPlayerId)
        {
            return string.IsNullOrWhiteSpace(targetPlayerId)
                ? ServiceResult.Fail(GameErrorCode.TargetNotFound)
                : ServiceResult.Success();
        }
    }
}

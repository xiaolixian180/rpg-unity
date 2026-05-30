using HeroQuest.Domain;

namespace HeroQuest.Systems.PvP
{
    public interface IPvpService
    {
        ServiceResult<PvpRewardPreview> PreviewKillReward(int targetGold, int targetKillValue);
        ServiceResult Attack(PvpAttackRequest request);
        ServiceResult ClaimBounty(string targetPlayerId);
        ServiceResult Revenge(string targetPlayerId);
    }
}

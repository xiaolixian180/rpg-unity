using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;

namespace HeroQuest.Systems.PvP
{
    /// <summary>
    /// PvP 服务——真实网络实现。
    /// 通过 NetworkManager 发送 PvpAttack/BountyHunt/Revenge 请求，await 响应事件，映射为业务域模型。
    /// PreviewKillReward 为纯客户端预览计算（基于 PvpRules），无服务端往返。
    /// </summary>
    public sealed class PvpService : IPvpService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public PvpService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public Task<ServiceResult<PvpRewardPreview>> PreviewKillReward(int targetGold, int targetKillValue)
        {
            // 击杀奖励预览为纯客户端计算（基于 PvpRules），无需服务端往返。
            // 实际奖励以 Attack 响应（GoPvpResult.gold_gain/honor_gain）为准。
            var preview = new PvpRewardPreview
            {
                goldReward = PvpRules.CalculateRobbedGold(targetGold, 0.1f),
                honorReward = 10,
                killValueDelta = 1,
                targetBecomesRedName = PvpRules.IsRedName(targetKillValue, 5)
            };

            return Task.FromResult(ServiceResult<PvpRewardPreview>.Success(preview));
        }

        public async Task<ServiceResult> Attack(PvpAttackRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.targetId))
            {
                return ServiceResult.Fail(GameErrorCode.TargetNotFound);
            }

            if (!ulong.TryParse(request.targetId, out var targetId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid targetId: {request.targetId}");
            }

            // attackerId 由服务端根据连接推断，本地忽略；skillId 可选（缺省 0=普通攻击）。
            int skillId = 0;
            if (!string.IsNullOrWhiteSpace(request.skillId))
            {
                if (!int.TryParse(request.skillId, out var parsedSkillId))
                {
                    return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid skillId: {request.skillId}");
                }

                skillId = parsedSkillId;
            }

            var result = await bridge.RequestAsync<GoPvpResult>(
                ct => networkManager.SendPvpAttackAsync(targetId, skillId, ct),
                h => networkManager.PvpAttackResult += h,
                h => networkManager.PvpAttackResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ClaimBounty(string targetPlayerId)
        {
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return ServiceResult.Fail(GameErrorCode.TargetNotFound);
            }

            if (!ulong.TryParse(targetPlayerId, out var targetId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid targetPlayerId: {targetPlayerId}");
            }

            var result = await bridge.RequestAsync<GoBountyReward>(
                ct => networkManager.SendBountyHuntAsync(targetId, ct),
                h => networkManager.BountyRewardReceived += h,
                h => networkManager.BountyRewardReceived -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> Revenge(string targetPlayerId)
        {
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                return ServiceResult.Fail(GameErrorCode.TargetNotFound);
            }

            if (!ulong.TryParse(targetPlayerId, out var targetId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid targetPlayerId: {targetPlayerId}");
            }

            var result = await bridge.RequestAsync<GoRevengeResponse>(
                ct => networkManager.SendRevengeAsync(targetId, ct),
                h => networkManager.RevengeResult += h,
                h => networkManager.RevengeResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            return ServiceResult.Success();
        }
    }
}

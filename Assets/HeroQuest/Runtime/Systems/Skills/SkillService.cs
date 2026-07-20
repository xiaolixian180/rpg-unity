using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.Skills
{
    /// <summary>
    /// 技能服务——真实网络实现。
    /// 通过 NetworkManager 发送 SkillLevelUp/SkillReset 请求，await 响应事件，映射为业务域模型。
    /// GetSnapshot 无独立查询协议，本地返回空快照。
    /// Cast 为广播型消息，采用 fire-and-forget；技能效果由战斗层订阅 SkillEffectReceived 处理。
    /// </summary>
    public sealed class SkillService : ISkillService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public SkillService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public Task<ServiceResult<SkillSnapshot>> GetSnapshot()
        {
            // 技能数据无独立查询协议，由服务端在技能升级/重置响应中隐式同步。
            // 此处返回空快照；实际技能列表由玩家数据同步层维护。
            return Task.FromResult(ServiceResult<SkillSnapshot>.Success(new SkillSnapshot()));
        }

        public async Task<ServiceResult> LearnOrUpgrade(string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return ServiceResult.Fail(GameErrorCode.SkillNotFound, "技能编号不能为空。");
            }

            if (!int.TryParse(skillId, out var skillIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid skillId: {skillId}");
            }

            var result = await bridge.RequestAsync<GoSkillLevelUpResponse>(
                ct => networkManager.SendSkillLevelUpAsync(skillIdValue, ct),
                h => networkManager.SkillLevelUpResult += h,
                h => networkManager.SkillLevelUpResult -= h);

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

        public async Task<ServiceResult> Cast(string skillId, string targetId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return ServiceResult.Fail(GameErrorCode.SkillNotFound, "技能编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                return ServiceResult.Fail(GameErrorCode.TargetNotFound, "目标编号不能为空。");
            }

            if (!int.TryParse(skillId, out var skillIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid skillId: {skillId}");
            }

            if (!ulong.TryParse(targetId, out var targetIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid targetId: {targetId}");
            }

            if (!bridge.IsConnected)
            {
                return ServiceResult.Fail(GameErrorCode.NetworkDisconnected);
            }

            // 技能释放为广播型消息：服务端处理后通过 SkillEffectReceived 推送给区域内所有玩家，
            // 无独立的 per-caster 响应码，故此处采用 fire-and-forget：仅确认请求发送成功。
            // 实际技能效果由战斗层订阅 SkillEffectReceived 事件另行处理。
            try
            {
                await networkManager.SendSkillCastAsync(skillIdValue, targetIdValue, 0, 0, default);
                return ServiceResult.Success();
            }
            catch (OperationCanceledException)
            {
                return ServiceResult.Fail(GameErrorCode.NetworkTimeout);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SkillService] Cast failed: {ex}");
                return ServiceResult.Fail(GameErrorCode.NetworkSendFailed, ex.Message);
            }
        }

        public async Task<ServiceResult> ResetAll(int goldCost)
        {
            if (goldCost < 0)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, "金币消耗不能为负数。");
            }

            var result = await bridge.RequestAsync<GoSkillResetResponse>(
                ct => networkManager.SendSkillResetAsync(ct),
                h => networkManager.SkillResetResult += h,
                h => networkManager.SkillResetResult -= h);

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

using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;

namespace HeroQuest.Systems.Pets
{
    /// <summary>
    /// 宠物服务——真实网络实现。
    /// 召唤/收回/升级/探索/合成均通过 NetworkManager 发送请求并 await 响应。
    /// </summary>
    public sealed class PetService : IPetService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public PetService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public Task<ServiceResult<PetSnapshot>> GetSnapshot()
        {
            return Task.FromResult(ServiceResult<PetSnapshot>.Success(new PetSnapshot()));
        }

        public async Task<ServiceResult> Summon(string petId)
        {
            if (!ulong.TryParse(petId, out var uid))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid petId: {petId}");
            }

            var result = await bridge.RequestAsync<GoPetSummonResponse>(
                ct => networkManager.SendPetSummonAsync(uid, ct),
                h => networkManager.PetSummonResult += h,
                h => networkManager.PetSummonResult -= h);

            if (!result.IsSuccess) return ServiceResult.Fail(result.ErrorCode, result.Message);
            return result.Value.code != 0
                ? ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(result.Value.code))
                : ServiceResult.Success();
        }

        public async Task<ServiceResult> Retract(string petId)
        {
            if (!ulong.TryParse(petId, out var uid))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid petId: {petId}");
            }

            var result = await bridge.RequestAsync<GoPetRecallResponse>(
                ct => networkManager.SendPetRecallAsync(uid, ct),
                h => networkManager.PetRecallResult += h,
                h => networkManager.PetRecallResult -= h);

            if (!result.IsSuccess) return ServiceResult.Fail(result.ErrorCode, result.Message);
            return result.Value.code != 0
                ? ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(result.Value.code))
                : ServiceResult.Success();
        }

        public async Task<ServiceResult> LevelUp(string petId)
        {
            if (!ulong.TryParse(petId, out var uid))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid petId: {petId}");
            }

            var result = await bridge.RequestAsync<GoPetLevelUpResponse>(
                ct => networkManager.SendPetLevelUpAsync(uid, ct),
                h => networkManager.PetLevelUpResult += h,
                h => networkManager.PetLevelUpResult -= h);

            if (!result.IsSuccess) return ServiceResult.Fail(result.ErrorCode, result.Message);
            return result.Value.code != 0
                ? ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(result.Value.code))
                : ServiceResult.Success();
        }

        public async Task<ServiceResult> StartExplore(string petId, int duration)
        {
            if (!ulong.TryParse(petId, out var uid))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid petId: {petId}");
            }

            var result = await bridge.RequestAsync<GoPetExploreResponse>(
                ct => networkManager.SendPetExploreAsync(uid, duration, ct),
                h => networkManager.PetExploreResult += h,
                h => networkManager.PetExploreResult -= h);

            if (!result.IsSuccess) return ServiceResult.Fail(result.ErrorCode, result.Message);
            return result.Value.code != 0
                ? ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(result.Value.code))
                : ServiceResult.Success();
        }

        public Task<ServiceResult> FinishExplore(string petId)
        {
            // 无"结束探索"协议：探索由服务端到期自动完成
            return Task.FromResult(ServiceResult.Success());
        }

        public async Task<ServiceResult> Synthesize(string[] materialPetIds)
        {
            if (materialPetIds == null || materialPetIds.Length == 0)
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter);
            }

            var uids = new ulong[materialPetIds.Length];
            for (int i = 0; i < materialPetIds.Length; i++)
            {
                if (!ulong.TryParse(materialPetIds[i], out uids[i]))
                {
                    return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid petId: {materialPetIds[i]}");
                }
            }

            var result = await bridge.RequestAsync<GoPetComposeResponse>(
                ct => networkManager.SendPetComposeAsync(uids, ct),
                h => networkManager.PetComposeResult += h,
                h => networkManager.PetComposeResult -= h);

            if (!result.IsSuccess) return ServiceResult.Fail(result.ErrorCode, result.Message);
            return result.Value.code != 0
                ? ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(result.Value.code))
                : ServiceResult.Success();
        }
    }
}

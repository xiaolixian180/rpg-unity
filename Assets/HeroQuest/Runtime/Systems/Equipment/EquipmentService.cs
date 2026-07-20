using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.Equipment
{
    /// <summary>
    /// 装备服务——真实网络实现。
    /// 通过 NetworkManager 发送 EquipWear/EquipUnload/EquipStrengthen/EquipEnchant 请求，
    /// await 响应事件，映射为业务域模型。
    /// GetSnapshot 无独立查询协议（装备数据随 GoPlayerData 同步），本地返回空快照。
    /// </summary>
    public sealed class EquipmentService : IEquipmentService
    {
        private readonly NetworkManager networkManager;
        private readonly NetworkAsyncBridge bridge;

        public EquipmentService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            bridge = new NetworkAsyncBridge(networkManager);
        }

        public Task<ServiceResult<EquipmentSnapshot>> GetSnapshot()
        {
            // 装备数据随 GoPlayerData.equipment 同步下发，无独立的装备列表查询协议。
            // 此处返回空快照；实际装备数据由登录/玩家数据同步层维护。
            return Task.FromResult(ServiceResult<EquipmentSnapshot>.Success(new EquipmentSnapshot()));
        }

        public async Task<ServiceResult> Equip(string equipId, EquipmentSlot slot)
        {
            if (string.IsNullOrWhiteSpace(equipId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, "装备编号不能为空。");
            }

            if (!ulong.TryParse(equipId, out var equipIdValue))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid equipId: {equipId}");
            }

            int slotIndex = (int)slot;

            var result = await bridge.RequestAsync<GoEquipWearResponse>(
                ct => networkManager.SendEquipWearAsync(slotIndex, equipIdValue, ct),
                h => networkManager.EquipWearResult += h,
                h => networkManager.EquipWearResult -= h);

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

        public async Task<ServiceResult> Unequip(EquipmentSlot slot)
        {
            int slotIndex = (int)slot;

            var result = await bridge.RequestAsync<GoEquipUnloadResponse>(
                ct => networkManager.SendEquipUnloadAsync(slotIndex, ct),
                h => networkManager.EquipUnloadResult += h,
                h => networkManager.EquipUnloadResult -= h);

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

        public async Task<ServiceResult> Strengthen(string equipId)
        {
            if (string.IsNullOrWhiteSpace(equipId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, "装备编号不能为空。");
            }

            // 协议强化以槽位为单位：将 equipId 解析为槽位编号。
            if (!int.TryParse(equipId, out var slot) || !Enum.IsDefined(typeof(EquipmentSlot), slot))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid equipId (slot): {equipId}");
            }

            var result = await bridge.RequestAsync<GoEquipStrengthenResponse>(
                ct => networkManager.SendEquipStrengthenAsync(slot, ct),
                h => networkManager.EquipStrengthenResult += h,
                h => networkManager.EquipStrengthenResult -= h);

            if (!result.IsSuccess)
            {
                return ServiceResult.Fail(result.ErrorCode, result.Message);
            }

            var response = result.Value;
            if (response.code != 0)
            {
                return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(response.code));
            }

            // 服务端返回 is_success=false 表示强化未成功（如幸运值不足）
            if (!response.is_success)
            {
                return ServiceResult.Fail(GameErrorCode.StrengthenFailed);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> Enchant(string equipId, string attributeId)
        {
            if (string.IsNullOrWhiteSpace(equipId) || string.IsNullOrWhiteSpace(attributeId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, "装备编号和属性编号不能为空。");
            }

            // 协议附魔以槽位为单位，并需附魔材料：equipId→slot，attributeId→material_id。
            if (!int.TryParse(equipId, out var slot) || !Enum.IsDefined(typeof(EquipmentSlot), slot))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid equipId (slot): {equipId}");
            }

            if (!ulong.TryParse(attributeId, out var materialId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid attributeId (material): {attributeId}");
            }

            var result = await bridge.RequestAsync<GoEquipEnchantResponse>(
                ct => networkManager.SendEquipEnchantAsync(slot, materialId, ct),
                h => networkManager.EquipEnchantResult += h,
                h => networkManager.EquipEnchantResult -= h);

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

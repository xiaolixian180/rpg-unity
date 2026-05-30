using HeroQuest.Domain;

namespace HeroQuest.Systems.Equipment
{
    public sealed class EquipmentServiceStub : IEquipmentService
    {
        public ServiceResult<EquipmentSnapshot> GetSnapshot()
        {
            return ServiceResult<EquipmentSnapshot>.Success(new EquipmentSnapshot());
        }

        public ServiceResult Equip(string equipId, EquipmentSlot slot)
        {
            return string.IsNullOrWhiteSpace(equipId)
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter, "Equipment id is required.")
                : ServiceResult.Success();
        }

        public ServiceResult Unequip(EquipmentSlot slot)
        {
            return ServiceResult.Success();
        }

        public ServiceResult Strengthen(string equipId)
        {
            return string.IsNullOrWhiteSpace(equipId)
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter, "Equipment id is required.")
                : ServiceResult.Success();
        }

        public ServiceResult Enchant(string equipId, string attributeId)
        {
            return string.IsNullOrWhiteSpace(equipId) || string.IsNullOrWhiteSpace(attributeId)
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter, "Equipment id and attribute id are required.")
                : ServiceResult.Success();
        }
    }
}

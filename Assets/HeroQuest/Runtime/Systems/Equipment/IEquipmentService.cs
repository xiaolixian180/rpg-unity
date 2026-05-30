using HeroQuest.Domain;

namespace HeroQuest.Systems.Equipment
{
    public interface IEquipmentService
    {
        ServiceResult<EquipmentSnapshot> GetSnapshot();
        ServiceResult Equip(string equipId, EquipmentSlot slot);
        ServiceResult Unequip(EquipmentSlot slot);
        ServiceResult Strengthen(string equipId);
        ServiceResult Enchant(string equipId, string attributeId);
    }
}

using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Equipment
{
    [Serializable]
    public sealed class EquipmentItem
    {
        public string equipId;
        public string templateId;
        public EquipmentSlot slot;
        public Quality quality;
        public int strengthenLevel;
        public string enchantAttribute;
    }

    [Serializable]
    public sealed class EquipmentSnapshot
    {
        public EquipmentItem[] equipped = Array.Empty<EquipmentItem>();
    }
}

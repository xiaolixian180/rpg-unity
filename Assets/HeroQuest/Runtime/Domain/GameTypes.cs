namespace HeroQuest.Domain
{
    public enum CurrencyType
    {
        Gold = 0,
        Honor = 1
    }

    public enum ItemCategory
    {
        Material = 0,
        Equipment = 1,
        Consumable = 2,
        PetEgg = 3
    }

    public enum EquipmentSlot
    {
        Weapon = 0,
        Helmet = 1,
        Armor = 2,
        Gloves = 3,
        Boots = 4,
        Necklace = 5,
        Ring1 = 6,
        Ring2 = 7
    }

    public enum Quality
    {
        Common = 0,
        Fine = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }

    public enum SkillKind
    {
        Active = 0,
        Passive = 1,
        Ultimate = 2
    }

    public enum PetKind
    {
        Attack = 0,
        Defense = 1,
        Support = 2,
        Resource = 3
    }

    public enum TradeOrderStatus
    {
        Listed = 0,
        Sold = 1,
        Cancelled = 2,
        Expired = 3
    }

    public enum RankingType
    {
        Level = 0,
        Power = 1,
        Honor = 2
    }
}

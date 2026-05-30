using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Inventory
{
    [Serializable]
    public sealed class InventoryItemStack
    {
        public string itemId;
        public ItemCategory category;
        public int count;
    }

    [Serializable]
    public sealed class InventorySnapshot
    {
        public InventoryItemStack[] items = Array.Empty<InventoryItemStack>();
    }
}

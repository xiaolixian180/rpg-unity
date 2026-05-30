using HeroQuest.Domain;

namespace HeroQuest.Systems.Inventory
{
    public interface IInventoryService
    {
        ServiceResult<InventorySnapshot> GetSnapshot();
        ServiceResult AddItem(string itemId, ItemCategory category, int count);
        ServiceResult ConsumeItem(string itemId, int count);
    }
}

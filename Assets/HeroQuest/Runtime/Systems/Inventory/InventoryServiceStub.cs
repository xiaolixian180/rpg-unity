using HeroQuest.Domain;

namespace HeroQuest.Systems.Inventory
{
    public sealed class InventoryServiceStub : IInventoryService
    {
        public ServiceResult<InventorySnapshot> GetSnapshot()
        {
            return ServiceResult<InventorySnapshot>.Success(new InventorySnapshot());
        }

        public ServiceResult AddItem(string itemId, ItemCategory category, int count)
        {
            return InventoryRules.IsValidCount(count)
                ? ServiceResult.Success()
                : ServiceResult.Fail(GameErrorCode.InvalidParameter, "Item count must be positive.");
        }

        public ServiceResult ConsumeItem(string itemId, int count)
        {
            return InventoryRules.IsValidCount(count)
                ? ServiceResult.Success()
                : ServiceResult.Fail(GameErrorCode.InvalidParameter, "Item count must be positive.");
        }
    }
}

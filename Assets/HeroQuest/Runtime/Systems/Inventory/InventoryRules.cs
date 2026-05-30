namespace HeroQuest.Systems.Inventory
{
    public static class InventoryRules
    {
        public static bool IsValidCount(int count)
        {
            return count > 0;
        }
    }
}

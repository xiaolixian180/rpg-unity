using HeroQuest.Domain;

namespace HeroQuest.Systems.Dungeon
{
    public static class DungeonResourceRules
    {
        public static GameErrorCode CanCollect(DungeonResourceNode node)
        {
            if (node == null)
            {
                return GameErrorCode.TargetNotFound;
            }

            if (node.isCollected || node.remainingCount <= 0)
            {
                return GameErrorCode.ResourceCollected;
            }

            return GameErrorCode.Success;
        }
    }
}

using HeroQuest.Domain;

namespace HeroQuest.Systems.Dungeon
{
    public interface IDungeonResourceService
    {
        ServiceResult<DungeonResourceNode> GetNode(string nodeId);
        ServiceResult Collect(string nodeId);
    }
}

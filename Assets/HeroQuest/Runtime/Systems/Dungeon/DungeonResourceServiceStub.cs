using HeroQuest.Domain;

namespace HeroQuest.Systems.Dungeon
{
    public sealed class DungeonResourceServiceStub : IDungeonResourceService
    {
        public ServiceResult<DungeonResourceNode> GetNode(string nodeId)
        {
            return string.IsNullOrWhiteSpace(nodeId)
                ? ServiceResult<DungeonResourceNode>.Fail(GameErrorCode.TargetNotFound)
                : ServiceResult<DungeonResourceNode>.Success(new DungeonResourceNode { nodeId = nodeId });
        }

        public ServiceResult Collect(string nodeId)
        {
            return string.IsNullOrWhiteSpace(nodeId)
                ? ServiceResult.Fail(GameErrorCode.TargetNotFound)
                : ServiceResult.Success();
        }
    }
}

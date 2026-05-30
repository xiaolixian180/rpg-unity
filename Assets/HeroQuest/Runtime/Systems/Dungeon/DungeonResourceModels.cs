using System;

namespace HeroQuest.Systems.Dungeon
{
    [Serializable]
    public sealed class DungeonResourceNode
    {
        public string nodeId;
        public string resourceId;
        public int layer;
        public int remainingCount;
        public bool isCollected;
    }
}

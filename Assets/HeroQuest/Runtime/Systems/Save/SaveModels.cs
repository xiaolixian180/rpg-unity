using System;

namespace HeroQuest.Systems.Save
{
    [Serializable]
    public sealed class SaveState
    {
        public string playerId;
        public long serverUnixSeconds;
        public bool isDirty;
    }
}

using HeroQuest.Domain;

namespace HeroQuest.Systems.Save
{
    public interface ISaveService
    {
        ServiceResult MarkDirty(string playerId);
        ServiceResult SaveNow(string playerId);
        ServiceResult<SaveState> GetState(string playerId);
    }
}

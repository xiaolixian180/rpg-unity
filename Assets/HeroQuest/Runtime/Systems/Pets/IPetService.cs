using HeroQuest.Domain;

namespace HeroQuest.Systems.Pets
{
    public interface IPetService
    {
        ServiceResult<PetSnapshot> GetSnapshot();
        ServiceResult Summon(string petId);
        ServiceResult Retract(string petId);
        ServiceResult LevelUp(string petId);
        ServiceResult StartExplore(string petId);
        ServiceResult FinishExplore(string petId);
        ServiceResult Synthesize(string[] materialPetIds);
    }
}

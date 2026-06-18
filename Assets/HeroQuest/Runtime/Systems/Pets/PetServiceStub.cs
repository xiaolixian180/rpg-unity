using HeroQuest.Domain;

namespace HeroQuest.Systems.Pets
{
    public sealed class PetServiceStub : IPetService
    {
        public ServiceResult<PetSnapshot> GetSnapshot()
        {
            return ServiceResult<PetSnapshot>.Success(new PetSnapshot());
        }

        public ServiceResult Summon(string petId) => ValidatePetId(petId);
        public ServiceResult Retract(string petId) => ValidatePetId(petId);
        public ServiceResult LevelUp(string petId) => ValidatePetId(petId);
        public ServiceResult StartExplore(string petId) => ValidatePetId(petId);
        public ServiceResult FinishExplore(string petId) => ValidatePetId(petId);

        public ServiceResult Synthesize(string[] materialPetIds)
        {
            return materialPetIds == null || materialPetIds.Length != PetRules.SynthesizeMaterialCount
                ? ServiceResult.Fail(GameErrorCode.PetSynthesizeMaterialCountInvalid)
                : ServiceResult.Success();
        }

        private static ServiceResult ValidatePetId(string petId)
        {
            return string.IsNullOrWhiteSpace(petId)
                ? ServiceResult.Fail(GameErrorCode.PetNotFound, "宠物编号不能为空。")
                : ServiceResult.Success();
        }
    }
}

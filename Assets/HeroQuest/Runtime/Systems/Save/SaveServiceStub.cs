using System;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Save
{
    public sealed class SaveServiceStub : ISaveService
    {
        public ServiceResult MarkDirty(string playerId) => ValidatePlayerId(playerId);
        public ServiceResult SaveNow(string playerId) => ValidatePlayerId(playerId);

        public ServiceResult<SaveState> GetState(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return ServiceResult<SaveState>.Fail(GameErrorCode.InvalidParameter);
            }

            return ServiceResult<SaveState>.Success(new SaveState
            {
                playerId = playerId,
                serverUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                isDirty = false
            });
        }

        private static ServiceResult ValidatePlayerId(string playerId)
        {
            return string.IsNullOrWhiteSpace(playerId)
                ? ServiceResult.Fail(GameErrorCode.InvalidParameter)
                : ServiceResult.Success();
        }
    }
}

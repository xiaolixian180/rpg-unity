namespace HeroQuest.Net.Auth
{
    public readonly struct AuthResult
    {
        public AuthResult(bool success, string playerId, string message)
        {
            Success = success;
            PlayerId = playerId;
            Message = message;
        }

        public bool Success { get; }
        public string PlayerId { get; }
        public string Message { get; }
    }
}

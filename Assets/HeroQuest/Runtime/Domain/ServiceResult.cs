namespace HeroQuest.Domain
{
    public readonly struct ServiceResult
    {
        public ServiceResult(GameErrorCode errorCode, string message = "")
        {
            ErrorCode = errorCode;
            Message = message;
        }

        public GameErrorCode ErrorCode { get; }
        public string Message { get; }
        public bool IsSuccess => ErrorCode == GameErrorCode.Success;

        public static ServiceResult Success()
        {
            return new ServiceResult(GameErrorCode.Success);
        }

        public static ServiceResult Fail(GameErrorCode errorCode, string message = "")
        {
            return new ServiceResult(errorCode, message);
        }
    }

    public readonly struct ServiceResult<T>
    {
        public ServiceResult(GameErrorCode errorCode, T value, string message = "")
        {
            ErrorCode = errorCode;
            Value = value;
            Message = message;
        }

        public GameErrorCode ErrorCode { get; }
        public T Value { get; }
        public string Message { get; }
        public bool IsSuccess => ErrorCode == GameErrorCode.Success;

        public static ServiceResult<T> Success(T value)
        {
            return new ServiceResult<T>(GameErrorCode.Success, value);
        }

        public static ServiceResult<T> Fail(GameErrorCode errorCode, string message = "")
        {
            return new ServiceResult<T>(errorCode, default, message);
        }
    }
}

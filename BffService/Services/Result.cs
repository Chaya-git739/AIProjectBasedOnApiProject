namespace BffService.Services;

public class Result<TSuccess, TError>
{
    public TSuccess? Success { get; init; }
    public TError? Error { get; init; }

    public bool IsSuccess => Success != null;

    public TR Match<TR>(Func<TSuccess, TR> onSuccess, Func<TError, TR> onError)
        => IsSuccess ? onSuccess(Success!) : onError(Error!);

    public static Result<TSuccess, TError> FromSuccess(TSuccess s) => new() { Success = s };
    public static Result<TSuccess, TError> FromError(TError e) => new() { Error = e };
}

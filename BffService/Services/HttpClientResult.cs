namespace BffService.Services;

public sealed class HttpClientResult<T>
{
    public T? Payload { get; }
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string? ErrorMessage { get; }

    public HttpClientResult(T? payload, bool isSuccess, int statusCode, string? errorMessage = null)
    {
        Payload = payload;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }
}

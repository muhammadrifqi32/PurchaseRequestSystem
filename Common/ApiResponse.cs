namespace PurchaseRequestSystem.Common;

/// <summary>
/// Standard envelope returned by every endpoint.
/// </summary>
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public ApiResponse() { }

    public ApiResponse(int statusCode, string message, T? data = default)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public static ApiResponse<T> Success(T? data, string message = "Request successful", int statusCode = 200)
        => new(statusCode, message, data);

    public static ApiResponse<T> Created(T? data, string message = "Resource created successfully")
        => new(201, message, data);

    public static ApiResponse<T> Fail(int statusCode, string message, T? data = default)
        => new(statusCode, message, data);
}

/// <summary>
/// Non-generic helper for responses that carry no payload.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object> Ok(string message = "Request successful")
        => new(200, message, null);

    public static ApiResponse<object> NotFound(string message = "Data not found")
        => new(404, message, null);

    public static ApiResponse<object> Error(string message = "An unexpected system error occurred. Please contact administrator.")
        => new(500, message, null);
}

/// <summary>
/// Payload shape used when returning a list of validation messages.
/// </summary>
public class ValidationErrorData
{
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PurchaseRequestSystem.Common;
using PurchaseRequestSystem.Common.Exceptions;

namespace PurchaseRequestSystem.Middlewares;

/// <summary>
/// Catches everything, logs it, and emits a consistent ApiResponse envelope.
/// Stack traces are never sent to the client.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, data) = Resolve(ex);

        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("Handled exception ({StatusCode}) on {Path}: {Message}", statusCode, context.Request.Path, ex.Message);

        var response = new ApiResponse<object?>(statusCode, message, data);

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private (int statusCode, string message, object? data) Resolve(Exception ex)
    {
        switch (ex)
        {
            case ValidationException ve:
                return (ve.StatusCode, ve.Message, new ValidationErrorData { Errors = ve.Errors });

            case AppException appEx:
                return (appEx.StatusCode, appEx.Message, null);

            case UnauthorizedAccessException:
                return (401, "Unauthorized", null);

            case DbUpdateException dbEx when IsUniqueViolation(dbEx):
                return (400, "A record with the same unique value already exists.", null);

            case DbUpdateException:
                return (400, "The operation could not be completed due to a data constraint violation.", null);

            default:
                // Never leak internals in production.
                var msg = _env.IsDevelopment()
                    ? $"{ex.Message}"
                    : "An unexpected system error occurred. Please contact administrator.";
                return (500, msg, null);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is SqlException sqlEx
           && sqlEx.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
}


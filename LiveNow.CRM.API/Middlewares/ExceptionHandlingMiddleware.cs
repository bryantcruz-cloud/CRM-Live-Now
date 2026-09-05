using System.Text.Json;
using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.API.Middlewares;

/// <summary>
/// Global exception handling. Converts domain exceptions to consistent,
/// client-safe JSON responses. Internal details (stack traces, EF Core errors)
/// are never exposed; unexpected errors are logged and returned as HTTP 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning("Error de negocio {ErrorCode}: {Message}", ex.ErrorCode, ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected; nothing to return.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);

            string message = _environment.IsDevelopment()
                ? ex.Message
                : "Ocurrió un error inesperado. Inténtelo de nuevo más tarde.";

            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", message);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        object payload = new
        {
            success = false,
            message,
            errorCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

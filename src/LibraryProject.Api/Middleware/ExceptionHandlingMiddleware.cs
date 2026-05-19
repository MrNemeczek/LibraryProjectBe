using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApplicationExceptionBase exception) when (!context.Response.HasStarted)
        {
            logger.LogWarning(exception, "Application exception occurred while processing request.");

            await WriteErrorResponseAsync(
                context,
                exception.StatusCode,
                exception.Code,
                exception.UserMessage,
                exception.Errors);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            logger.LogError(exception, "Unhandled exception occurred while processing request.");

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                new Dictionary<string, string[]>());
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse(statusCode, code, message, errors, context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(response, cancellationToken: context.RequestAborted);
    }

    private sealed record ErrorResponse(
        int StatusCode,
        string Code,
        string Message,
        IReadOnlyDictionary<string, string[]> Errors,
        string TraceId);
}

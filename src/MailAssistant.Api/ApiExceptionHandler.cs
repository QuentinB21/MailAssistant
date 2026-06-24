using Microsoft.AspNetCore.Diagnostics;

namespace MailAssistant.Api;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Request conflict"),
            _ => (0, string.Empty),
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await Results.Problem(
                statusCode: statusCode,
                title: title,
                detail: exception.Message)
            .ExecuteAsync(httpContext);

        return true;
    }
}

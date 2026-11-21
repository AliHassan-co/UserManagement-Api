using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace UserManagementApi.Middleware;

public class RequestLoggingAndExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingAndExceptionMiddleware> _logger;

    public RequestLoggingAndExceptionMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingAndExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            _logger.LogInformation("Incoming request: {Method} {Path}", method, path);
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "Completed request: {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
                method,
                path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path} after {ElapsedMs}ms",
                method,
                path,
                stopwatch.ElapsedMilliseconds);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = JsonSerializer.Serialize(new
            {
                title = "An unexpected error occurred.",
                status = context.Response.StatusCode
            });

            await context.Response.WriteAsync(errorResponse);
        }
    }
}
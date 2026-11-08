using System.Net;
using System.Text.Json;
using eBike.Application.Common.DTOs;
using eBike.Application.Common.Exceptions;

namespace eBike.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "One or more validation failures occurred.",
                validationEx.Errors.SelectMany(x => x.Value).ToList()
            ),
            BadRequestException badRequestEx => (
                (int)HttpStatusCode.BadRequest,
                badRequestEx.Message,
                new List<string> { badRequestEx.Message }
            ),
            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<string> { notFoundEx.Message }
            ),
            ConflictException conflictEx => (
                (int)HttpStatusCode.Conflict,
                conflictEx.Message,
                new List<string> { conflictEx.Message }
            ),
            ForbiddenException forbiddenEx => (
                (int)HttpStatusCode.Forbidden,
                forbiddenEx.Message,
                new List<string> { forbiddenEx.Message }
            ),
            UnauthorizedAccessException unauthorizedEx => (
                (int)HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                new List<string> { unauthorizedEx.Message }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected internal error occurred. Please contact system support.",
                GetAllExceptionMessages(exception)
            )
        };

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static List<string> GetAllExceptionMessages(Exception ex)
    {
        var list = new List<string>();
        var current = ex;
        while (current != null)
        {
            if (!string.IsNullOrWhiteSpace(current.Message) && !list.Contains(current.Message))
            {
                list.Add(current.Message);
            }
            current = current.InnerException;
        }
        return list;
    }
}

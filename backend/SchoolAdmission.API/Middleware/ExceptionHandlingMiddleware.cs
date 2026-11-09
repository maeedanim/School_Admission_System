
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Exceptions;

namespace SchoolAdmission.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (AdmissionException exception)
        {
            _logger.LogWarning(
                "Admission error {ErrorCode}: {Message}",
                exception.Code,
                exception.Message);

            await WriteErrorResponseAsync(
                context,
                GetStatusCode(exception.Code),
                exception.Code,
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unexpected error occurred while processing the request.");

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static int GetStatusCode(string errorCode)
    {
        return errorCode switch
        {
            "INVALID_STUDENT_ID" => StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,

            "NOT_FOUND" => StatusCodes.Status404NotFound,

            "NOT_OWNER" => StatusCodes.Status403Forbidden,

            "SEATS_UNAVAILABLE" => StatusCodes.Status409Conflict,
            "ALREADY_APPLIED" => StatusCodes.Status409Conflict,
            "HOLD_EXPIRED" => StatusCodes.Status409Conflict,
            "HOLD_NOT_ACTIVE" => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException(
                "The response has already started.");
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = errorCode,
            Detail = message,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = errorCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem));
    }
}

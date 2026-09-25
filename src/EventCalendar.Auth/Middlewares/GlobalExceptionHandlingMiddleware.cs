using EventCalendar.Auth.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Auth.Middlewares;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception. Method={Method}, Path={Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            if (httpContext.Response.HasStarted)
                return;

            var statusCode = ex switch
            {
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                LoginAlreadyExistsException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Detail = ex.Message
            });
        }
    }
}

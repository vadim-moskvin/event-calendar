using System.ComponentModel.DataAnnotations;
using EventCalendar.Bookings.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Bookings.Middlewares;

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
                ValidationException or BadRequestException or ArgumentException => StatusCodes.Status400BadRequest,
                NotAllowedException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                MaxBookingPerUserException => StatusCodes.Status409Conflict,
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

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventCalendar.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        if (method.IsDefined(typeof(AllowAnonymousAttribute), true) ||
            method.DeclaringType?.IsDefined(typeof(AllowAnonymousAttribute), true) == true)
            return;

        if (!method.IsDefined(typeof(AuthorizeAttribute), true) &&
            method.DeclaringType?.IsDefined(typeof(AuthorizeAttribute), true) != true)
            return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Authorize = YouShallNotPassBackend.Security.AuthorizeAttribute;

public class AuthorizationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        Type? type = context?.MethodInfo?.DeclaringType;
        if (type == null)
        {
            operation.Security.Clear();
            return;
        }

        Authorize? attribute = type
            .GetCustomAttributes(true)
            .Union(context!.MethodInfo.GetCustomAttributes(true))
            .OfType<Authorize>()
            .FirstOrDefault();

        if (attribute == null)
        {
            operation.Security.Clear();
            return;
        }

        operation.Security = new List<OpenApiSecurityRequirement>()
        {
            new OpenApiSecurityRequirement
            { 
                {
                    new() { Reference = new() { Id = "Bearer", Type = ReferenceType.SecurityScheme } }, new List<string>()
                } 
            }
        };
    }
}
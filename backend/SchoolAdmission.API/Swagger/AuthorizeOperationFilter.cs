using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolAdmission.API.Swagger;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var endpointMetadata =
            context.ApiDescription.ActionDescriptor.EndpointMetadata;

        // Anonymous endpoints must not require a Bearer token in Swagger.
        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        // Add Bearer authentication only to protected endpoints.
        if (!endpointMetadata.OfType<IAuthorizeData>().Any())
        {
            return;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    "Bearer",
                    context.Document)] = []
            });
    }
}

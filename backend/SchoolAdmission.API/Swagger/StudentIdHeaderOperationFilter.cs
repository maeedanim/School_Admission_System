
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SchoolAdmission.API.Swagger;

public class StudentIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod;
        var path = context.ApiDescription.RelativePath;

        if (string.IsNullOrWhiteSpace(method) ||
            string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        path = path.Trim('/');

        bool requiresStudentId =
            (method == "POST" &&
             path.StartsWith("classes/", StringComparison.OrdinalIgnoreCase) &&
             path.EndsWith("/holds", StringComparison.OrdinalIgnoreCase))
            ||
            (method == "POST" &&
             path.StartsWith("holds/", StringComparison.OrdinalIgnoreCase) &&
             path.EndsWith("/confirm", StringComparison.OrdinalIgnoreCase))
            ||
            (method == "DELETE" &&
             path.StartsWith("holds/", StringComparison.OrdinalIgnoreCase))
            ||
            (method == "GET" &&
             path.Equals("me/holds", StringComparison.OrdinalIgnoreCase));

        if (!requiresStudentId)
        {
            return;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    "StudentId",
                    context.Document)] = []
            });
    }
}

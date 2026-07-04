using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SVSim.EmulatedEntrypoint.Infrastructure;

/// <summary>
/// Attaches the <see cref="RequireAdminSecretAttribute.HeaderName"/> security requirement to
/// Swagger operations whose action carries <see cref="RequireAdminSecretAttribute"/>. The
/// matching <see cref="OpenApiSecurityScheme"/> is registered by <c>Program.cs</c> under the
/// same scheme id (<see cref="SchemeId"/>) so the Swagger UI shows an Authorize dialog and,
/// once populated, sends the header on gated endpoints only.
/// </summary>
public sealed class AdminSecretOperationFilter : IOperationFilter
{
    public const string SchemeId = "AdminSecret";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute = context.MethodInfo.GetCustomAttributes(true)
            .OfType<RequireAdminSecretAttribute>().Any()
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<RequireAdminSecretAttribute>().Any() ?? false);

        if (!hasAttribute) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = SchemeId,
                }
            }] = Array.Empty<string>()
        });
    }
}

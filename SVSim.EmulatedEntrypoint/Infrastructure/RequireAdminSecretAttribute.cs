using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SVSim.EmulatedEntrypoint.Configuration;

namespace SVSim.EmulatedEntrypoint.Infrastructure;

/// <summary>
/// Gates a controller or action on a shared secret carried in the <c>X-Admin-Secret</c> header,
/// compared against <see cref="AdminOptions.ImportSecret"/>. Runs as an authorization filter so
/// unauthorized requests short-circuit before model binding.
///
/// Fail-closed: if the configured secret is null/empty the endpoint is treated as disabled and
/// every request gets a 401 (with a warning logged once per request). This means a deployment
/// that forgets to set the secret leaves the endpoint locked, not open.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class RequireAdminSecretAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Admin-Secret";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var services = context.HttpContext.RequestServices;
        var options = services.GetRequiredService<IOptions<AdminOptions>>().Value;
        var logger = services.GetRequiredService<ILogger<RequireAdminSecretAttribute>>();

        if (string.IsNullOrWhiteSpace(options.ImportSecret))
        {
            logger.LogWarning(
                "Rejecting request to {Path}: Admin:ImportSecret is not configured. " +
                "Set it in appsettings (or the NPGSQL_ADMIN__IMPORTSECRET env var) to enable the endpoint.",
                context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || provided.Count == 0
            || string.IsNullOrEmpty(provided[0]))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var providedBytes = Encoding.UTF8.GetBytes(provided[0]!);
        var expectedBytes = Encoding.UTF8.GetBytes(options.ImportSecret);
        if (!CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}

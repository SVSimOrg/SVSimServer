namespace SVSim.EmulatedEntrypoint.Configuration;

/// <summary>
/// Config for the /admin/* util endpoints. Bound from the "Admin" section of appsettings.
/// </summary>
public class AdminOptions
{
    public const string SectionName = "Admin";

    /// <summary>
    /// Shared secret required in the <c>X-Admin-Secret</c> header on protected admin endpoints
    /// (see <see cref="Infrastructure.RequireAdminSecretAttribute"/>). Empty / whitespace means
    /// the endpoint is disabled — the filter fails closed so an unconfigured deployment never
    /// exposes the admin surface.
    /// </summary>
    public string ImportSecret { get; set; } = string.Empty;
}

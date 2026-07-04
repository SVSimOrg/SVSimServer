using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace SVSim.Hosting;

public static class SerilogHostingExtensions
{
    // Detects `dotnet test` runs regardless of ASPNETCORE_ENVIRONMENT wiring. The env check
    // via HostingEnvironment.IsEnvironment("Testing") is unreliable here because
    // WebApplicationFactory<TEntryPoint> sets the environment via a mechanism that fires after
    // both CreateBuilder and the UseSerilog callback resolve their environment view. The entry
    // assembly under NUnit + dotnet test is always "testhost" or "testhost.x86".
    private static readonly bool IsTestHost =
        Assembly.GetEntryAssembly()?.GetName().Name?.StartsWith("testhost", StringComparison.OrdinalIgnoreCase) == true;

    // Wires Serilog as the host's logging provider. Sinks, rotation, retention,
    // and enrichment defaults are baked in here; per-category level overrides
    // ride via appsettings under "Serilog:MinimumLevel:Override". The file sink
    // rolls daily (or when a single file hits 100 MB) and retains the newest 7.
    public static IHostBuilder UseSvSimSerilog(this IHostBuilder host, string appName)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        var filePathBase = Path.Combine(logDirectory, $"{appName}-.log");

        return host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");

            // Skip the file sink under `dotnet test` so unit-test runs don't scatter log files
            // under SVSim.UnitTests/bin/. See IsTestHost above for why we detect via entry
            // assembly instead of the ASP.NET environment name.
            if (!IsTestHost)
            {
                loggerConfig.WriteTo.File(
                    path: filePathBase,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 100_000_000,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
            }

            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });
    }
}

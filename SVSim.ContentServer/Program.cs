using Microsoft.Extensions.FileProviders;
using Serilog;
using SVSim.Hosting;

namespace SVSim.ContentServer;

// SVSim.ContentServer — server #3 of the 4-server topology.
//
// Dumb static-file host serving the Shadowverse asset CDN tree. The CDN is
// plain HTTP, no auth, content-addressed by MD5 in the Resource/Sound trees
// and name-keyed under Manifest/{resver}/{lang}/{plat}/. We mirror the URL
// structure on disk and mount UseStaticFiles directly at /dl/.
//
// Required runtime config:
//   SVSIM_CONTENT_ROOT  env var pointing at the asset root (must contain a
//                       dl/ subdirectory built by data_dumps/scripts/
//                       content_cdn_mirror.py).
//
// Alternatively the same path may be set in appsettings.json under
// "Content:Root", or passed on the command line as --Content:Root=<path>.
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSvSimSerilog("svsim-content");

        try
        {
        // Env var takes priority — it's the canonical knob per the spec docs.
        var contentRoot = Environment.GetEnvironmentVariable("SVSIM_CONTENT_ROOT")
            ?? builder.Configuration["Content:Root"];

        if (string.IsNullOrWhiteSpace(contentRoot))
        {
            Console.Error.WriteLine(
                "SVSim.ContentServer: no content root configured. Set SVSIM_CONTENT_ROOT " +
                "env var, Content:Root in appsettings, or pass --Content:Root=<path>.");
            Environment.Exit(2);
        }

        contentRoot = Path.GetFullPath(contentRoot);
        var dlRoot = Path.Combine(contentRoot, "dl");

        if (!Directory.Exists(dlRoot))
        {
            Console.Error.WriteLine(
                $"SVSim.ContentServer: content root '{contentRoot}' does not contain a 'dl' " +
                "subdirectory. Run data_dumps/scripts/content_cdn_mirror.py to populate.");
            Environment.Exit(2);
        }

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        // Request logging for first-light visibility. Registered before
        // UseStaticFiles so it observes the static responses too.
        app.Use(async (context, next) =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await next();
            sw.Stop();
            logger.LogInformation("{Status} {Method} {Path} ({Elapsed}ms)",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path.Value,
                sw.ElapsedMilliseconds);
        });

        // ServeUnknownFileTypes=true is REQUIRED. Blob filenames are bare MD5
        // hashes with no extension; asset/sound/movie extensions (.unity3d,
        // .acb, .awb, .usm, .lz4) aren't in the default MIME map. The client
        // doesn't care about Content-Type, but it does care about getting
        // non-zero bytes.
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(dlRoot),
            RequestPath = "/dl",
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream",
        });

        app.MapGet("/health", () => "ok");

        logger.LogInformation("Content root: {Root}", dlRoot);
        logger.LogInformation("Serving /dl/* from this root. Manifests are under " +
                              "/dl/Manifest/{{resver}}/{{lang}}/{{plat}}/; blob trees are " +
                              "content-addressed under /dl/Resource/ and /dl/Sound/.");

        app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

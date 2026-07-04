using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Reads a JSON seed file under <c>SVSim.Bootstrap/Data/seeds/</c>. Replaces ImporterBase.LoadCapture.
/// Files are produced by extractors in <c>data_dumps/scripts/</c>; the bootstrap project does not
/// transform wire formats. Missing files are non-fatal (returns empty/null) — caller decides.
/// </summary>
public static class SeedLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static List<T> LoadList<T>(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[SeedLoader] Missing seed file: {path}");
            return new List<T>();
        }
        using var fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<List<T>>(fs, Options) ?? new List<T>();
    }

    public static T? LoadObject<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[SeedLoader] Missing seed file: {path}");
            return null;
        }
        using var fs = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(fs, Options);
    }
}

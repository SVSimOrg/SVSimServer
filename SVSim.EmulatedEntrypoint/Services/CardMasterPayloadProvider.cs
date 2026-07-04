using Microsoft.Extensions.Logging;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Reads the static card-master blob from the app's output directory once at startup and
/// hands the cached base64 string to <c>ImmutableDataController</c>. Singleton because the
/// file is ~1.27 MB and gzip-decoding it on every request would burn CPU for no benefit.
/// </summary>
public interface ICardMasterPayloadProvider
{
    /// <summary>True when the blob loaded successfully and serving is enabled.</summary>
    bool IsAvailable { get; }

    /// <summary>The verbatim base64 string ready for <c>data.card_master</c>.</summary>
    string Base64Blob { get; }
}

public sealed class CardMasterPayloadProvider : ICardMasterPayloadProvider
{
    // Filename is pinned to the captured snapshot's date. When swapping the blob, update
    // this constant AND the hash in CardMasterConfig in the same change.
    private const string BlobFileName = "card_master_2026-06-03.txt";

    public bool IsAvailable { get; }
    public string Base64Blob { get; }

    public CardMasterPayloadProvider(ILogger<CardMasterPayloadProvider> log)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", BlobFileName);
        if (!File.Exists(path))
        {
            log.LogWarning("Card-master blob missing at {Path} — /immutable_data/card_master will 503.", path);
            Base64Blob = "";
            IsAvailable = false;
            return;
        }
        string content;
        try
        {
            content = File.ReadAllText(path).Trim();
        }
        catch (IOException ex)
        {
            log.LogError(ex, "Failed reading card-master blob at {Path} — /immutable_data/card_master will 503.", path);
            Base64Blob = "";
            IsAvailable = false;
            return;
        }
        if (content.Length == 0)
        {
            log.LogWarning("Card-master blob at {Path} is empty — /immutable_data/card_master will 503.", path);
            Base64Blob = "";
            IsAvailable = false;
            return;
        }
        Base64Blob = content;
        IsAvailable = true;
        log.LogInformation("Loaded card-master blob from {Path} ({Bytes} base64 chars).", path, Base64Blob.Length);
    }
}

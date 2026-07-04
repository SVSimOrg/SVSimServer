namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Tiny shared helper for content importers. Capture parsing has moved out of the bootstrap
/// project entirely (extractors under <c>data_dumps/scripts/</c> emit per-table seed JSON);
/// only the wire-date normaliser stays here because several seed-driven importers still need
/// to canonicalise prod-shaped timestamp strings.
/// </summary>
public static class ImporterBase
{
    /// <summary>Parse a wire date that may be ISO ("2026-05-23T..."), space-separated ("2026-05-23 16:32:31"), or empty.</summary>
    public static DateTime ParseWireDateTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return DateTime.MinValue;
        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            return dt;
        }
        return DateTime.MinValue;
    }
}

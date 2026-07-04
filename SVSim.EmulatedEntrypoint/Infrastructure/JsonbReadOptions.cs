using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Infrastructure;

/// <summary>
/// Shared System.Text.Json options for deserializing jsonb-passthrough columns into typed DTOs.
///
/// The seed-driven globals JSON was written with snake_case_lower keys (see SVSim.Bootstrap
/// per-domain importers — jsonb columns store the original wire-shape verbatim). Deserialize-back must
/// use the same naming policy so e.g. `card_pool_name` maps onto `CardPoolName`.
///
/// AllowReadingFromString handles prod's PHP-backend convention of emitting numeric values
/// as JSON strings (e.g. `"ability_id": "1"`). Numeric-typed DTO properties accept those.
///
/// Used by LoadController and MyPageController (and any other controller that reads jsonb).
/// </summary>
public static class JsonbReadOptions
{
    public static readonly JsonSerializerOptions Instance = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = false,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };
}

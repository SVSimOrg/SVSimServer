using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One row per top-level game-config section. <see cref="SectionName"/> matches the
/// <c>ConfigSectionAttribute.Name</c> on the corresponding POCO in <c>Models.Config</c>
/// (e.g. <c>"PackRates"</c> → <c>PackRateConfig</c>). <see cref="ValueJson"/> is the section's
/// payload, stored as <c>jsonb</c> on Postgres and <c>TEXT</c> on SQLite.
/// <para>
/// Deserialisation goes through pure System.Text.Json in <c>IGameConfigService</c> — EF doesn't
/// know about the section POCOs. Replaces the old single-row <c>GameConfigurations</c> table
/// (one wide jsonb document, EF Core 8 <c>OwnsOne</c>+<c>ToJson</c> tree). See ADR-pending /
/// 2026-05-24 config-refactor discussion for the why.
/// </para>
/// </summary>
public class GameConfigSection : ITimeTrackedEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string SectionName { get; set; } = "";

    /// <summary>Raw JSON payload for this section. Postgres stores as jsonb; SQLite as TEXT.</summary>
    public string ValueJson { get; set; } = "{}";

    public DateTime DateCreated { get; set; } = DateTime.MinValue;
    public DateTime? DateUpdated { get; set; }
}

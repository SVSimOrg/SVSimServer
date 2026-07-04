using SVSim.Database.Models;

namespace SVSim.Database.Services.BattleXp;

/// <summary>
/// Amounts returned to callers after a class-XP grant. <see cref="TotalXp"/> and
/// <see cref="Level"/> are POST-grant, POST-level-up (matching the wire shape's
/// <c>class_experience</c> + <c>class_level</c> post-state semantics).
/// <see cref="LeveledUp"/> is <c>true</c> iff at least one level threshold was
/// crossed during this grant — callers gate <c>class_level_up</c> mission emits
/// on this flag rather than caching pre-state themselves.
/// </summary>
public sealed record BattleXpGrantResult(int GetXp, int TotalXp, int Level, bool LeveledUp);

public interface IBattleXpService
{
    /// <summary>
    /// Grants class XP for a battle finish. Caller supplies a viewer loaded via
    /// <see cref="Repositories.Viewer.IViewerRepository.LoadForBattleXpGrantAsync"/>
    /// (or equivalent, with <c>.Include(v =&gt; v.Classes).ThenInclude(c =&gt; c.Class)</c>).
    /// Caller <c>SaveChangesAsync</c> after this returns.
    /// <para>
    /// Amount resolution: mode-specific config override if non-null, else global
    /// <c>XpPerWin</c>/<c>XpPerLoss</c>. Story ignores <paramref name="isWin"/> (always
    /// treated as a clear).
    /// </para>
    /// <para>
    /// Level-up: loops on <c>ClassExpEntry</c> curve; <c>row.Exp</c> stores level-relative
    /// XP and carries overflow after each level-up. Saturates at curve max level (excess
    /// piles in <c>Exp</c>).
    /// </para>
    /// <para>
    /// Guardrail: viewer has no <see cref="ViewerClassData"/> row for
    /// <paramref name="classId"/> → returns <c>(0, 0, 1)</c>, no mutation, logs Warning.
    /// </para>
    /// </summary>
    Task<BattleXpGrantResult> GrantAsync(
        Viewer viewer, int classId, bool isWin, BattleXpMode mode, CancellationToken ct = default);
}

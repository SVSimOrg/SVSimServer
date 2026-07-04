using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SVSim.Database.Models;

namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Caller-supplied extra <c>.Include</c> chains on top of the canonical viewer-inventory query
/// in <see cref="IInventoryService.BeginAsync"/>. Use to bring in extra collections needed by
/// the calling controller (e.g. <c>MissionData</c>, <c>BuildDeckPurchases</c>).
/// <para>
/// Also carries the <see cref="Source"/> tag that <see cref="IInventoryTransaction.CommitAsync"/>
/// stamps onto every <c>viewer_acquire_history</c> row written from this transaction. Callers
/// that don't set <see cref="Source"/> end up with <see cref="GrantSource.Unknown"/> rows;
/// grep for <c>acquire_type=0</c> in dev to find unmigrated sites.
/// </para>
/// </summary>
public sealed class InventoryLoadConfig
{
    internal List<Func<IQueryable<Viewer>, IQueryable<Viewer>>> Includes { get; } = new();

    /// <summary>
    /// Logical source of every grant queued in this transaction. Defaults to
    /// <see cref="GrantSource.Unknown"/>.
    /// </summary>
    public GrantSource Source { get; set; } = GrantSource.Unknown;

    public InventoryLoadConfig WithInclude<TProperty>(
        Expression<Func<Viewer, TProperty>> path)
    {
        Includes.Add(q => q.Include(path));
        return this;
    }

    public InventoryLoadConfig WithInclude<TProperty, TThen>(
        Expression<Func<Viewer, IEnumerable<TProperty>>> collectionPath,
        Expression<Func<TProperty, TThen>> thenPath)
    {
        Includes.Add(q => q.Include(collectionPath).ThenInclude(thenPath));
        return this;
    }
}

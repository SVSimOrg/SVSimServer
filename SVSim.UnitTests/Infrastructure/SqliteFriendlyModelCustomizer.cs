using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SVSim.Database.Models;

namespace SVSim.UnitTests.Infrastructure;

/// <summary>
/// Replaces the default <see cref="ModelCustomizer"/> in tests. After the normal
/// <c>OnModelCreating</c> runs, strips the Postgres sequence the production model declares
/// for <c>Viewer.ShortUdid</c> so EnsureCreated can build the schema against SQLite (which
/// has no sequence support).
/// </summary>
internal class SqliteFriendlyModelCustomizer : ModelCustomizer
{
    public SqliteFriendlyModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        modelBuilder.Model.RemoveSequence("ShortUdidSequence");

        var shortUdidProperty = modelBuilder.Entity<Viewer>().Property(v => v.ShortUdid).Metadata;
        shortUdidProperty.RemoveAnnotation("Relational:DefaultValueSql");
        shortUdidProperty.ValueGenerated = ValueGenerated.Never;

        AssignClientSideKeyGenerators(modelBuilder.Model);
    }

    /// <summary>
    /// Owned-collection shadow PKs are <c>ValueGenerated.OnAdd</c> with the production model
    /// expecting the database to auto-fill (Postgres IDENTITY). On SQLite a composite-PK column
    /// is not a ROWID alias, so the DB can't auto-fill it and we get NOT NULL violations. Walk
    /// every owned entity and swap any auto-add primary-key property to use an in-process
    /// counter instead.
    /// </summary>
    private static void AssignClientSideKeyGenerators(IMutableModel model)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (!entityType.IsOwned()) continue;

            foreach (var key in entityType.GetKeys())
            {
                foreach (var property in key.Properties)
                {
                    if (property.ValueGenerated != ValueGenerated.OnAdd) continue;
                    if (property.ClrType != typeof(int) && property.ClrType != typeof(long)) continue;

                    property.SetValueGeneratorFactory((_, _) =>
                        property.ClrType == typeof(int)
                            ? (ValueGenerator)new MonotonicIntValueGenerator()
                            : new MonotonicLongValueGenerator());
                }
            }
        }
    }
}

internal sealed class MonotonicIntValueGenerator : ValueGenerator<int>
{
    private static int _current;
    public override bool GeneratesTemporaryValues => false;
    public override int Next(EntityEntry entry) => Interlocked.Increment(ref _current);
}

internal sealed class MonotonicLongValueGenerator : ValueGenerator<long>
{
    private static long _current;
    public override bool GeneratesTemporaryValues => false;
    public override long Next(EntityEntry entry) => Interlocked.Increment(ref _current);
}

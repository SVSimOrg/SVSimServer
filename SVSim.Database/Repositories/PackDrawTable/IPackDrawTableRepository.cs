namespace SVSim.Database.Repositories.PackDrawTables;

public interface IPackDrawTableRepository
{
    /// <summary>Returns the draw table for <paramref name="packId"/>, or null if not seeded.</summary>
    Task<PackDrawTable?> GetAsync(int packId);
}

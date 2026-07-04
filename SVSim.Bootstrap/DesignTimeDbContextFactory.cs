using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using SVSim.Database;

namespace SVSim.Bootstrap;

/// <summary>
/// Lets `dotnet ef migrations add` instantiate SVSimDbContext at design time. The runtime ctor
/// takes an ILogger which EF's tooling can't resolve without DI; this factory bypasses that.
/// Connection string here only needs to be valid Npgsql syntax — EF doesn't actually connect
/// during migration scaffolding.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SVSimDbContext>
{
    public SVSimDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SVSimDbContext>()
            .UseNpgsql("Host=localhost;Database=svsim;Username=postgres;password=postgres")
            .Options;
        return new SVSimDbContext(NullLogger<SVSimDbContext>.Instance, options);
    }
}

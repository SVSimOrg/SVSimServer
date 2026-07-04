using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Repositories;

public abstract class BaseRepository<T> where T : class
{
    protected readonly SVSimDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public BaseRepository(SVSimDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = DbContext.Set<T>();
    }
}
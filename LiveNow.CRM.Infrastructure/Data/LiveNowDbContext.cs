using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.Infrastructure.Data;

public class LiveNowDbContext : DbContext
{
    public LiveNowDbContext(DbContextOptions<LiveNowDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LiveNowDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Infrastructure.Data;
using LiveNow.CRM.Infrastructure.Repositories;

namespace LiveNow.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        string? databaseProvider = configuration["DatabaseProvider"]?.ToLowerInvariant();
        string migrationsAssembly = databaseProvider == "postgresql"
            ? "LiveNow.CRM.Infrastructure.PostgreSql"
            : "LiveNow.CRM.Infrastructure";

        services.AddDbContext<LiveNowDbContext>(options =>
        {
            switch (databaseProvider)
            {
                case "sqlite":
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                    break;
                case "postgresql":
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                    break;
                default:
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                    break;
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}

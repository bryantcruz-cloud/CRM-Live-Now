using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Infrastructure.Data;

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

        services.AddDbContext<LiveNowDbContext>(options =>
        {
            switch (databaseProvider)
            {
                case "sqlite":
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                    break;
                case "postgresql":
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                    break;
                default:
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                    break;
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

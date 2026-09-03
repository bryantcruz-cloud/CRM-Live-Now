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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var databaseProvider = configuration.GetValue<string>("DatabaseProvider")?.ToLowerInvariant();

        services.AddDbContext<LiveNowDbContext>(options =>
        {
            switch (databaseProvider)
            {
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString);
                    break;
                case "postgresql":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LiveNow.CRM.API;

/// <summary>
/// Creates the context for EF Core tooling without starting the API host.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LiveNowDbContext>
{
    public LiveNowDbContext CreateDbContext(string[] args)
    {
        string environment = GetEnvironment(args);
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine("LiveNow.CRM.API", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("LiveNow.CRM.API", $"appsettings.{environment}.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "EF Core design-time tooling requires ConnectionStrings__DefaultConnection.");
        }

        string? provider = configuration["DatabaseProvider"]?.Trim().ToLowerInvariant();
        DbContextOptionsBuilder<LiveNowDbContext> optionsBuilder = new();

        switch (provider)
        {
            case "postgresql":
                optionsBuilder.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsAssembly("LiveNow.CRM.Infrastructure.PostgreSql"));
                break;
            case "sqlite":
                optionsBuilder.UseSqlite(
                    connectionString,
                    sqlite => sqlite.MigrationsAssembly("LiveNow.CRM.Infrastructure"));
                break;
            default:
                throw new InvalidOperationException(
                    "EF Core design-time tooling requires DatabaseProvider=postgresql or DatabaseProvider=sqlite.");
        }

        return new LiveNowDbContext(optionsBuilder.Options);
    }

    private static string GetEnvironment(string[] args)
    {
        for (int index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], "--environment", StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }
}

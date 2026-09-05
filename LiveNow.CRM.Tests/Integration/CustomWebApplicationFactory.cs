using LiveNow.CRM.Infrastructure;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveNow.CRM.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that replaces the SQLite connection with a fresh
/// in-memory database per test run. Each test gets a clean database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-only-key-do-not-use-in-production-1234567890",
            ["Jwt:Issuer"] = "LiveNow.Tests",
            ["Jwt:Audience"] = "LiveNow.Tests",
            ["Jwt:ExpirationMinutes"] = "60"
        }));

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LiveNow.CRM.Infrastructure.Data.LiveNowDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Create a new in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<LiveNow.CRM.Infrastructure.Data.LiveNowDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LiveNow.CRM.Infrastructure.Data.LiveNowDbContext>();
            db.Database.EnsureCreated();
            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Name = "Test Admin",
                    Username = "testadmin",
                    Email = "testadmin@local.test",
                    PasswordHash = UserService.HashPassword("Test-password-1234567890"),
                    Role = "Admin"
                });
                db.SaveChanges();
            }
        });

        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "test-only-key-do-not-use-in-production-1234567890");
        builder.UseSetting("Jwt:Issuer", "LiveNow.Tests");
        builder.UseSetting("Jwt:Audience", "LiveNow.Tests");
        builder.UseSetting("Jwt:ExpirationMinutes", "60");
    }

    protected override void Dispose(bool disposing)
    {
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose(disposing);
    }
}

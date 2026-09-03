using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Infrastructure.Data;

namespace LiveNow.CRM.Tests.Unit;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;

    protected LiveNowDbContext Context { get; private set; } = null!;

    protected TestBase()
    {
        // Use a unique in-memory database with a unique name to ensure isolation
        string databaseName = $"TestDb_{Guid.NewGuid():N}";
        string connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        var options = new DbContextOptionsBuilder<LiveNowDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new LiveNowDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Database?.EnsureDeleted();
        Context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}









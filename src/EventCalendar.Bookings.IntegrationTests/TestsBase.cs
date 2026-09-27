using EventCalendar.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventCalendar.Bookings.IntegrationTests;

public class TestsBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("eventcalendar_bookings_test")
        .Build();

    protected string ConnectionString => _postgres.GetConnectionString();

    public virtual async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected BookingsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new BookingsDbContext(options);
    }

    protected async Task ResetDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}

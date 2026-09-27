using EventCalendar.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventCalendar.Events.IntegrationTests;

public class TestsBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("eventcalendar_events_test")
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

    protected EventsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new EventsDbContext(options);
    }

    protected async Task ResetDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}

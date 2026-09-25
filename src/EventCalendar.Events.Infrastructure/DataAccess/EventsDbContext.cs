using EventCalendar.Events.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.Events.Infrastructure.DataAccess;

public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventsDbContext).Assembly);
    }
}

using EventCalendar.Events.Domain.Models;

namespace EventCalendar.Events.IntegrationTests.TestHelpers;

public static class TestServiceFactory
{
    public static Event MakeEvent(Guid? id = null, string? title = null, DateTime? startAt = null,
        DateTime? endAt = null, int? totalSeats = null)
    {
        id ??= Guid.NewGuid();
        title ??= "Название события";
        startAt ??= DateTime.UtcNow.AddDays(1);
        endAt ??= startAt + TimeSpan.FromHours(1);
        totalSeats ??= 100;

        return new Event(id.Value, title, startAt.Value, endAt.Value, totalSeats.Value);
    }
}

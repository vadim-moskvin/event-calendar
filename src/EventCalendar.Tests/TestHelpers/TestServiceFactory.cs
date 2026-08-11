using EventCalendar.Models;

namespace EventCalendar.Tests.TestHelpers;

public static class TestServiceFactory
{
    public static Event MakeEvent(Guid? id = null, string? title = null, DateTime? startAt = null,
        DateTime? endAt = null, int? totalSeats = null)
    {
        id ??= Guid.NewGuid();
        title ??= "Название события";
        startAt ??= DateTime.Now;
        endAt ??= startAt + TimeSpan.FromHours(1);
        totalSeats ??= 100;

        return new Event((Guid)id, title, (DateTime)startAt, (DateTime)endAt, (int)totalSeats);
    }
}
using EventCalendar.Models;
using EventCalendar.Services;

namespace EventCalendar.Tests.TestHelpers;

public static class TestServiceFactory
{
    public static EventService MakeEventService()
    {
        return new EventService(new InMemoryEventStore());
    }
    
    public static BookingService MakeBookingService(EventService eventService)
    {
        var bookingRepository = new InMemoryBookingStore();
        return new BookingService(eventService, bookingRepository);
    }

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
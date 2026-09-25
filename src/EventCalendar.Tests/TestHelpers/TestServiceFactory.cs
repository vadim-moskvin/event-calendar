using EventCalendar.Domain.Models;

namespace EventCalendar.Tests.TestHelpers;

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

        return new Event((Guid)id, title, (DateTime)startAt, (DateTime)endAt, (int)totalSeats);
    }

    public static Booking MakeBooking(Guid? userId = null, Guid? eventId = null)
    {
        userId ??= Guid.NewGuid();
        eventId ??= Guid.NewGuid();

        return Booking.MakeNew((Guid)userId, (Guid)eventId);
    }
    
    public static User MakeUser(Guid? id = null, string? login = null, string? hash = null, Role? role = null)
    {
        login ??= "login";
        hash ??= "hash";
        role ??= Role.User;

        return User.MakeNew(login, hash, (Role)role);
    }
}

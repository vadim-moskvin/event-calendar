using EventCalendar.Contracts;
using EventCalendar.Events.Application.Repositories;

namespace EventCalendar.Events.Application.Services;

public sealed class BookingConfirmationHandler(IEventRepository eventRepository) : IBookingConfirmationHandler
{
    public async Task<BookingConfirmationResult> HandleAsync(
        BookingConfirmed message, CancellationToken cancellationToken = default)
    {
        if (message.BookingId == Guid.Empty || message.EventId == Guid.Empty ||
            message.UserId == Guid.Empty || message.SeatCount <= 0)
            return BookingConfirmationResult.InvalidMessage;

        var @event = await eventRepository.GetEventAsync(message.EventId);
        if (@event is null)
            return BookingConfirmationResult.EventNotFound;

        if (!@event.TryReserveSeats(message.SeatCount))
            return BookingConfirmationResult.NoAvailableSeats;

        await eventRepository.SaveChangesAsync(cancellationToken);
        return BookingConfirmationResult.Reserved;
    }
}

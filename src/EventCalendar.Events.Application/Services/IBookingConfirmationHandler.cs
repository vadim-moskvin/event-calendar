using EventCalendar.Contracts;

namespace EventCalendar.Events.Application.Services;

public interface IBookingConfirmationHandler
{
    Task<BookingConfirmationResult> HandleAsync(
        BookingConfirmed message, CancellationToken cancellationToken = default);
}

using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Contracts;

namespace EventCalendar.Bookings.Application.Services;

public sealed class BookingProcessingService(
    IBookingRepository bookingRepository,
    IBookingConfirmedPublisher publisher)
    : IBookingProcessingService
{
    public async Task ProcessAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var booking = await bookingRepository.GetBookingAsync(bookingId);
        if (booking is null || booking.Status != BookingStatus.Pending)
            return;

        booking.Confirm();
        await bookingRepository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new BookingConfirmed(
            booking.Id,
            booking.EventId,
            booking.UserId,
            1,
            booking.ProcessedAt!.Value), cancellationToken);
    }
}

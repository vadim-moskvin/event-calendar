using EventCalendar.Bookings.Application.Repositories;
using EventCalendar.Bookings.Domain.Exceptions;
using EventCalendar.Bookings.Domain.Models;

namespace EventCalendar.Bookings.Application.Services;

public class BookingService(IBookingRepository bookingRepository) : IBookingService
{
    private const int MaxBookingPerUser = 10;

    private const string BookingNotFoundException = "Бронь не найдена";

    private static readonly SemaphoreSlim[] UserLocks = Enumerable.Range(0, 256)
        .Select(_ => new SemaphoreSlim(1, 1))
        .ToArray();

    public async Task<Booking> CreateBookingAsync(Guid userId, Guid eventId)
    {
        var semaphore = UserLocks[(int)((uint)userId.GetHashCode() % UserLocks.Length)];

        await semaphore.WaitAsync();

        try
        {
            var bookingCount = await bookingRepository
                .GetActiveBookingCountByUserIdAsync(userId);
            if (bookingCount >= MaxBookingPerUser)
                throw new MaxBookingPerUserException();

            var booking = Booking.MakeNew(userId, eventId);
            await bookingRepository.CreateBookingAsync(booking);
            await bookingRepository.SaveChangesAsync();

            return booking;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid userId, bool isAdmin)
    {
        var booking = await bookingRepository.GetBookingAsync(bookingId)
                      ?? throw new NotFoundException(BookingNotFoundException);

        var isOwner = booking.UserId == userId;
        if (!isOwner && !isAdmin)
            throw new NotAllowedException("Нет прав для удаления бронирования");

        booking.Cancel();
        await bookingRepository.SaveChangesAsync();
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, bool isAdmin)
    {
        var booking = await bookingRepository.GetBookingAsync(bookingId)
                      ?? throw new NotFoundException(BookingNotFoundException);

        if (booking.UserId != userId && !isAdmin)
            throw new NotAllowedException("Нет прав для просмотра бронирования");

        return booking;
    }
}

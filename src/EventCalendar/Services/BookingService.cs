using EventCalendar.Exceptions;
using EventCalendar.Models;
using EventCalendar.Repositories;

namespace EventCalendar.Services;

public class BookingService(IEventService eventService, IBookingRepository bookingRepository) : IBookingService
{
    private const string BookingNotFoundException = "Бронь не найдена";

    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        _ = eventService.GetEvent(eventId);
        var booking = Booking.MakeNew(eventId);
        await bookingRepository.CreateOrUpdateBookingAsync(booking);
        return booking;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
        return await bookingRepository.GetBookingByIdAsync(bookingId) ??
               throw new NotFoundException(BookingNotFoundException);
    }
}
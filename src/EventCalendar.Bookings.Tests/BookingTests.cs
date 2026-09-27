using EventCalendar.Bookings.Domain.Models;

namespace EventCalendar.Bookings.Tests;

public class BookingTests
{
    [Fact]
    public void Create_new_booking()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        // Act
        var booking = Booking.MakeNew(userId, eventId);

        // Assert
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(DateTime.UtcNow.Date, booking.CreatedAt.Date);
    }

    [Fact]
    public void Confirm()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var booking = Booking.MakeNew(userId, eventId);

        // Act
        booking.Confirm();

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public void Reject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var booking = Booking.MakeNew(userId, eventId);

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }
}
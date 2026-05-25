using EventCalendar.Models;

namespace EventCalendar.Tests;

public class BookingTests
{
    [Fact]
    public void Create_new_booking()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking = Booking.MakeNew(eventId);

        // Assert
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(DateTime.Today, booking.CreatedAt.Date);
    }
}
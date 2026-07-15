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

    [Fact]
    public void Confirm()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var booking = Booking.MakeNew(eventId);

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
        var eventId = Guid.NewGuid();
        var booking = Booking.MakeNew(eventId);

        // Act
        booking.Reject();
        
        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }
}
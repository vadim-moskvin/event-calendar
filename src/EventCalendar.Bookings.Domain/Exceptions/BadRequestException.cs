namespace EventCalendar.Bookings.Domain.Exceptions;

public class BadRequestException(string? message) : Exception(message);
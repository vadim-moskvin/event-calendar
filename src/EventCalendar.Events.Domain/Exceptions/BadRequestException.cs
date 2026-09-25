namespace EventCalendar.Events.Domain.Exceptions;

public class BadRequestException(string? message) : Exception(message);
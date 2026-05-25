namespace EventCalendar.Exceptions;

public class BadRequestException(string? message) : Exception(message);
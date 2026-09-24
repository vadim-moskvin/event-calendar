namespace EventCalendar.Domain.Exceptions;

public class UnauthorizedException(string message) : Exception(message);

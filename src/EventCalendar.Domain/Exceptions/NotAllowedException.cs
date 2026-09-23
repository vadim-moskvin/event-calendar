namespace EventCalendar.Domain.Exceptions;

public class NotAllowedException(string message) : Exception(message);
namespace EventCalendar.Events.Application;

public record PaginatedResult<T>(IEnumerable<T> Items, int CurrentPage, int TotalPages, int TotalItems);
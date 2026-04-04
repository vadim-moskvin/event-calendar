namespace EventCalendar.Services;

public sealed record CreateOrUpdateResult<T>(T Entity, bool Created);
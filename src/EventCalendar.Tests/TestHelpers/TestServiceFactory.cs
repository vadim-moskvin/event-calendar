using EventCalendar.Services;

namespace EventCalendar.Tests.TestHelpers;

public static class TestServiceFactory
{
    public static EventService MakeEventService()
    {
        return new EventService(new InMemoryEventStore());
    }
}
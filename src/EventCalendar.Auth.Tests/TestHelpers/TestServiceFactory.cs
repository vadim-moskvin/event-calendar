using EventCalendar.Auth.Domain.Models;

namespace EventCalendar.Auth.Tests.TestHelpers;

public static class TestServiceFactory
{
    public static User MakeUser(string login = "alice", string hash = "hash", Role role = Role.User) =>
        User.MakeNew(login, hash, role);
}

namespace EventCalendar.Domain.Models;

public class User(Guid id, string login, string passwordHash, Role role)
{
    public Guid Id { get; init; } = id;

    public string Login { get; init; } = login;

    public string PasswordHash { get; init; } = passwordHash;

    public Role Role { get; init; } = role;

    public static User MakeNew(string login, string passwordHash, Role role) =>
        new(Guid.NewGuid(), login, passwordHash, role);
}
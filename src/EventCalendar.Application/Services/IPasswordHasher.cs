namespace EventCalendar.Application.Services;

public interface IPasswordHasher
{
    string Hash(string password);

    bool CheckPassword(string password, string hash);
}
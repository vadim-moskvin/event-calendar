using System.Security.Cryptography;
using System.Text;
using EventCalendar.Application.Services;

namespace EventCalendar.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool CheckPassword(string password, string hash)
    {
        return Hash(password) == hash;
    }
}
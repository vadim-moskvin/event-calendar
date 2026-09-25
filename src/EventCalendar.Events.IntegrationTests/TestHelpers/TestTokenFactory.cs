using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EventCalendar.Events.IntegrationTests.TestHelpers;

public static class TestTokenFactory
{
    public const string SecretKey = "0123456789abcdef0123456789abcdef";

    public static string Create(Guid userId, string role, bool forEvents = true)
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "EventCalendar.Auth",
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId.ToString(),
                ["role"] = role
            },
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)), SecurityAlgorithms.HmacSha256)
        };
        descriptor.Audiences.Add(forEvents ? "EventCalendar.Events" : "EventCalendar.Bookings");
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}

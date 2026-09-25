using System.Text;
using EventCalendar.Application;
using EventCalendar.Application.Services;
using EventCalendar.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EventCalendar.Infrastructure.Services;

public class TokenService(IOptions<TokenSettings> options) : ITokenService
{
    private readonly TokenSettings _settings = options.Value;

    public string GenerateToken(Guid userId, string login, Role role)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            ["login"] = login,
            ["role"] = role.ToString()
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Claims = claims,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
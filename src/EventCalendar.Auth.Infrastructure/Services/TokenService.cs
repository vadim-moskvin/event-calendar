using System.Text;
using EventCalendar.Auth.Application;
using EventCalendar.Auth.Application.Services;
using EventCalendar.Auth.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EventCalendar.Auth.Infrastructure.Services;

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
            Claims = claims,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = creds
        };

        foreach (var audience in _settings.Audiences)
            descriptor.Audiences.Add(audience);

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}

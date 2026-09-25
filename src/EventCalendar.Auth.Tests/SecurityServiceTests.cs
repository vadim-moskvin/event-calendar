using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EventCalendar.Auth.Application;
using EventCalendar.Auth.Domain.Models;
using EventCalendar.Auth.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventCalendar.Auth.Tests;

public class SecurityServiceTests
{
    [Fact]
    public void Hash_and_check_password()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var hash = hasher.Hash("secret");
        var anotherHash = hasher.Hash("secret");

        // Assert
        Assert.NotEqual("secret", hash);
        Assert.StartsWith("pbkdf2-sha256$600000$", hash);
        Assert.NotEqual(hash, anotherHash);
        Assert.True(hasher.CheckPassword("secret", hash));
        Assert.True(hasher.CheckPassword("secret", anotherHash));
        Assert.False(hasher.CheckPassword("wrong", hash));
        Assert.False(hasher.CheckPassword("secret", "invalid-hash"));
    }

    [Theory]
    [InlineData("short", "issuer", "audience", 30)]
    [InlineData("0123456789abcdef0123456789abcdef", "", "audience", 30)]
    [InlineData("0123456789abcdef0123456789abcdef", "issuer", "", 30)]
    [InlineData("0123456789abcdef0123456789abcdef", "issuer", "audience", 0)]
    public void Validate_token_settings_with_invalid_values(string secretKey, string issuer, string audience, int expirationMinutes)
    {
        // Arrange
        var settings = new TokenSettings
        {
            SecretKey = secretKey,
            Issuer = issuer,
            Audiences = [audience],
            ExpirationMinutes = expirationMinutes
        };

        // Act
        Action action = settings.Validate;

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Generate_token()
    {
        // Arrange
        var settings = new TokenSettings
        {
            SecretKey = "0123456789abcdef0123456789abcdef",
            Issuer = "test-issuer",
            Audiences = ["EventCalendar.Events", "EventCalendar.Bookings"],
            ExpirationMinutes = 30
        };
        var id = Guid.NewGuid();
        settings.Validate();

        // Act
        var token = new TokenService(Options.Create(settings)).GenerateToken(id, "alice", Role.Admin);

        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = "EventCalendar.Events",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
                RoleClaimType = "role"
            }, out var validatedToken);

        // Assert
        Assert.Equal(id.ToString(), principal.FindFirst("sub")?.Value);
        Assert.Equal("alice", principal.FindFirst("login")?.Value);
        Assert.True(principal.IsInRole("Admin"));
        Assert.Contains("EventCalendar.Bookings", ((JwtSecurityToken)validatedToken).Audiences);
        Assert.True(validatedToken.ValidTo > DateTime.UtcNow);
    }
}

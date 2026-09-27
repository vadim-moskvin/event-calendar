using System.Text;

namespace EventCalendar.Bookings;

public sealed class JwtSettings
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || Encoding.UTF8.GetByteCount(SecretKey) < 32)
            throw new InvalidOperationException("TokenSettings:SecretKey must contain at least 32 UTF-8 bytes.");
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("TokenSettings:Issuer is required.");
        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("TokenSettings:Audience is required.");
    }
}

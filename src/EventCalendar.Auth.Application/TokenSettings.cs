using System.Text;

namespace EventCalendar.Auth.Application;

public sealed class TokenSettings
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string[] Audiences { get; init; }
    public required int ExpirationMinutes { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey) || Encoding.UTF8.GetByteCount(SecretKey) < 32)
            throw new InvalidOperationException("TokenSettings:SecretKey must contain at least 32 UTF-8 bytes.");
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("TokenSettings:Issuer is required.");
        if (Audiences is not { Length: > 0 } || Audiences.Any(string.IsNullOrWhiteSpace) ||
            Audiences.Distinct(StringComparer.Ordinal).Count() != Audiences.Length)
            throw new InvalidOperationException("TokenSettings:Audiences must contain unique, non-empty values.");
        if (ExpirationMinutes <= 0)
            throw new InvalidOperationException("TokenSettings:ExpirationMinutes must be positive.");
    }
}

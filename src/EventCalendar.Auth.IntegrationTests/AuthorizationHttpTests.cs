using System.Net;
using System.Net.Http.Json;
using EventCalendar.Auth.Controllers.Dtos;
using EventCalendar.Auth.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EventCalendar.Auth.IntegrationTests;

public class AuthorizationHttpTests : TestsBase
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string? _previousConnectionString;
    private string? _previousSecretKey;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        _previousSecretKey = Environment.GetEnvironmentVariable("TokenSettings__SecretKey");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", TestSecretKey);
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                    ["TokenSettings:SecretKey"] = TestSecretKey
                }));
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    public override async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _previousConnectionString);
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", _previousSecretKey);
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Register_and_login()
    {
        var login = $"user-{Guid.NewGuid():N}";
        var registration = new RegisterDto { Login = login, Password = "secret", Role = Role.User };

        var register = await _client.PostAsJsonAsync("/register", registration);
        var duplicate = await _client.PostAsJsonAsync("/register", registration);
        var success = await _client.PostAsJsonAsync("/login", new LoginDto
        {
            Login = login, Password = "secret"
        });
        var wrongPassword = await _client.PostAsJsonAsync("/login", new LoginDto
        {
            Login = login, Password = "wrong"
        });
        var unknownUser = await _client.PostAsJsonAsync("/login", new LoginDto
        {
            Login = $"unknown-{Guid.NewGuid():N}", Password = "secret"
        });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        var token = await success.Content.ReadFromJsonAsync<TokenDto>();
        Assert.NotNull(token);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.Token);
        Assert.Equal("EventCalendar.Auth", jwt.Issuer);
        Assert.Contains("EventCalendar.Events", jwt.Audiences);
        Assert.Contains("EventCalendar.Bookings", jwt.Audiences);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
    }

    private const string TestSecretKey = "0123456789abcdef0123456789abcdef";
}

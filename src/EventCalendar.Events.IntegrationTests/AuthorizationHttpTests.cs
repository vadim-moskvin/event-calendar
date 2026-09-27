using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventCalendar.Events.Controllers.Dtos;
using EventCalendar.Events.IntegrationTests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EventCalendar.Events.IntegrationTests;

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
        Environment.SetEnvironmentVariable("TokenSettings__SecretKey", TestTokenFactory.SecretKey);
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                    ["TokenSettings:SecretKey"] = TestTokenFactory.SecretKey
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
    public async Task Create_event_requires_admin_token_for_events()
    {
        using var user = AuthenticatedClient("User");
        using var admin = AuthenticatedClient("Admin");
        using var wrongAudience = AuthenticatedClient("Admin", forEvents: false);
        var eventDto = NewEvent();

        var anonymousResponse = await _client.PostAsJsonAsync("/events", eventDto);
        var userResponse = await user.PostAsJsonAsync("/events", eventDto);
        var wrongAudienceResponse = await wrongAudience.PostAsJsonAsync("/events", eventDto);
        var adminResponse = await admin.PostAsJsonAsync("/events", eventDto);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongAudienceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_can_delete_event()
    {
        using var admin = AuthenticatedClient("Admin");
        var create = await admin.PostAsJsonAsync("/events", NewEvent());
        var @event = await create.Content.ReadFromJsonAsync<GetEventDto>();
        Assert.NotNull(@event);

        var delete = await admin.DeleteAsync($"/events/{@event.Id}");
        var deleteAgain = await admin.DeleteAsync($"/events/{@event.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteAgain.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync($"/events/{@event.Id}")).StatusCode);
    }

    private HttpClient AuthenticatedClient(string role, bool forEvents = true)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            TestTokenFactory.Create(Guid.NewGuid(), role, forEvents));
        return client;
    }

    private static EventDto NewEvent() => new()
    {
        Title = "Authorization test event",
        StartAt = DateTime.UtcNow.AddDays(2),
        EndAt = DateTime.UtcNow.AddDays(2).AddHours(1),
        TotalSeats = 5
    };
}

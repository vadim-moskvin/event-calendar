using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventCalendar.Controllers.Dtos;
using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace EventCalendar.IntegrationTests;

public class AuthorizationHttpTests : TestsBase
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = ConnectionString
                }));
            builder.ConfigureServices(services => services.RemoveAll<IHostedService>());
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
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Register_and_login()
    {
        // Arrange
        var login = UniqueLogin();

        // Act
        var register = await _client.PostAsJsonAsync("/register", new RegisterDto
        {
            Login = login, Password = "secret", Role = Role.User
        });
        var duplicate = await _client.PostAsJsonAsync("/register", new RegisterDto
        {
            Login = login, Password = "secret", Role = Role.User
        });
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
            Login = UniqueLogin(), Password = "secret"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await success.Content.ReadFromJsonAsync<TokenDto>())?.Token));
        Assert.Equal(HttpStatusCode.Forbidden, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, unknownUser.StatusCode);
    }

    [Fact]
    public async Task Create_event_with_different_credentials()
    {
        // Arrange
        var eventDto = NewEvent();
        using var userClient = await AuthenticatedClient(Role.User);
        using var adminClient = await AuthenticatedClient(Role.Admin);

        // Act
        var anonymous = await _client.PostAsJsonAsync("/events", eventDto);
        var userResponse = await userClient.PostAsJsonAsync("/events", eventDto);
        var adminResponse = await adminClient.PostAsJsonAsync("/events", eventDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Cancel_another_users_booking()
    {
        // Arrange
        using var admin = await AuthenticatedClient(Role.Admin);
        var createEvent = await admin.PostAsJsonAsync("/events", NewEvent());
        Assert.Equal(HttpStatusCode.Created, createEvent.StatusCode);
        var createdEvent = await createEvent.Content.ReadFromJsonAsync<GetEventDto>();
        Assert.NotNull(createdEvent);

        using var owner = await AuthenticatedClient(Role.User);
        var createBooking = await owner.PostAsync($"/events/{createdEvent.Id}/book", null);
        Assert.Equal(HttpStatusCode.Accepted, createBooking.StatusCode);
        var booking = await createBooking.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(booking);

        using var otherUser = await AuthenticatedClient(Role.User);

        // Act
        var cancel = await otherUser.DeleteAsync($"/{booking.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, cancel.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.NotEqual(BookingStatus.Cancelled,
            (await db.Bookings.AsNoTracking().SingleAsync(b => b.Id == booking.Id)).Status);
    }

    private async Task<HttpClient> AuthenticatedClient(Role role)
    {
        var login = UniqueLogin();
        var registered = await _client.PostAsJsonAsync("/register", new RegisterDto
        {
            Login = login, Password = "secret", Role = role
        });
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        var response = await _client.PostAsJsonAsync("/login", new LoginDto
        {
            Login = login, Password = "secret"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        Assert.NotNull(token);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    private static string UniqueLogin() => $"user-{Guid.NewGuid():N}";

    private static EventDto NewEvent() => new()
    {
        Title = "Authorization test event",
        StartAt = DateTime.UtcNow.AddDays(2),
        EndAt = DateTime.UtcNow.AddDays(2).AddHours(1),
        TotalSeats = 5
    };
}

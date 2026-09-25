using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventCalendar.Bookings.Controllers.Dtos;
using EventCalendar.Bookings.Domain.Models;
using EventCalendar.Bookings.Infrastructure.DataAccess;
using EventCalendar.Bookings.IntegrationTests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Bookings.IntegrationTests;

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
    public async Task Booking_requires_token_for_bookings_and_owner_can_cancel()
    {
        var ownerId = Guid.NewGuid();
        using var owner = AuthenticatedClient(ownerId, "User");
        using var other = AuthenticatedClient(Guid.NewGuid(), "User");
        using var wrongAudience = AuthenticatedClient(ownerId, "User", forBookings: false);
        var request = new CreateBookingDto { EventId = Guid.NewGuid() };

        var anonymous = await _client.PostAsJsonAsync("/bookings", request);
        var wrongAudienceResponse = await wrongAudience.PostAsJsonAsync("/bookings", request);
        var create = await owner.PostAsJsonAsync("/bookings", request);
        var booking = await create.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(booking);

        var otherRead = await other.GetAsync($"/bookings/{booking.Id}");
        var otherCancel = await other.DeleteAsync($"/bookings/{booking.Id}");
        var ownerCancel = await owner.DeleteAsync($"/bookings/{booking.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongAudienceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, otherRead.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, otherCancel.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, ownerCancel.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        Assert.Equal(BookingStatus.Cancelled,
            (await db.Bookings.AsNoTracking().SingleAsync(x => x.Id == booking.Id)).Status);
    }

    [Fact]
    public async Task Admin_can_cancel_another_users_booking()
    {
        using var owner = AuthenticatedClient(Guid.NewGuid(), "User");
        using var admin = AuthenticatedClient(Guid.NewGuid(), "Admin");
        var create = await owner.PostAsJsonAsync("/bookings", new CreateBookingDto { EventId = Guid.NewGuid() });
        var booking = await create.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(booking);

        var response = await admin.DeleteAsync($"/bookings/{booking.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private HttpClient AuthenticatedClient(Guid userId, string role, bool forBookings = true)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            TestTokenFactory.Create(userId, role, forBookings));
        return client;
    }
}

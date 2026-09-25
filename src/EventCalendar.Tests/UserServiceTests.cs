using System.IdentityModel.Tokens.Jwt;
using EventCalendar.Application.Repositories;
using EventCalendar.Application.Services;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;
using EventCalendar.Infrastructure.DataAccess;
using EventCalendar.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCalendar.Tests;

public class UserServiceTests : TestsBase
{
    [Fact]
    public async Task Register()
    {
        // Arrange
        const string login = "alice";
        const string password = "secret";

        // Act
        var user = await UserService.Register(login, password, Role.User);

        // Assert
        using var scope = ServiceProvider.CreateScope();
        var persisted = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Users.AsNoTracking().SingleAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        Assert.Equal(user.Id, persisted.Id);
        Assert.Equal(login, persisted.Login);
        Assert.Equal(Role.User, persisted.Role);
        Assert.NotEqual(password, persisted.PasswordHash);
        Assert.True(hasher.CheckPassword(password, persisted.PasswordHash));
    }

    [Fact]
    public async Task Register_existing_login()
    {
        // Arrange
        var existing = TestServiceFactory.MakeUser(login: "alice");
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();
        await repository.CreateUserAsync(existing);

        // Act + Assert
        await Assert.ThrowsAsync<LoginAlreadyExistsException>(() => UserService.Register("alice", "secret", Role.User));
        Assert.Equal(existing.Id, (await repository.FindUserAsync("alice"))?.Id);
        Assert.Single(ServiceProvider.GetRequiredService<AppDbContext>().Users);
    }

    [Fact]
    public async Task Login()
    {
        // Arrange
        var hasher = ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = TestServiceFactory.MakeUser(login: "alice", hash: hasher.Hash("secret"), role: Role.Admin);
        await ServiceProvider.GetRequiredService<IUserRepository>().CreateUserAsync(user);

        // Act
        var token = await UserService.Login("alice", "secret");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == "sub").Value);
        Assert.Equal("alice", jwt.Claims.Single(c => c.Type == "login").Value);
        Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == "role").Value);
    }

    [Theory]
    [InlineData("unknown", "secret")]
    [InlineData("alice", "wrong")]
    public async Task Login_unknown_user_or_wrong_password(string login, string password)
    {
        // Arrange
        var hasher = ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = TestServiceFactory.MakeUser(login: "alice", hash: hasher.Hash("secret"));
        await ServiceProvider.GetRequiredService<IUserRepository>().CreateUserAsync(user);

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => UserService.Login(login, password));
    }
}

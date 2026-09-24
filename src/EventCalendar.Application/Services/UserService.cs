using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IUserService
{
    public async Task<User> Register(string login, string password, Role role)
    {
        var existingUser = await userRepository.FindUserAsync(login);
        if (existingUser != null)
            throw new LoginAlreadyExistsException();

        var hashedPassword = passwordHasher.Hash(password);
        var user = User.MakeNew(login, hashedPassword, role);
        await userRepository.CreateUserAsync(user);
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<string> Login(string login, string password)
    {
        var user = await userRepository.FindUserAsync(login);
        if (user == null)
            throw new UnauthorizedException("Неверный логин или пароль");

        if (!passwordHasher.CheckPassword(password, user.PasswordHash))
            throw new UnauthorizedException("Неверный логин или пароль");

        return tokenService.GenerateToken(user.Id, login, user.Role);
    }
}

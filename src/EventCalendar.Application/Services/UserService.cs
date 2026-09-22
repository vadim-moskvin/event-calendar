using EventCalendar.Application.Repositories;
using EventCalendar.Domain.Exceptions;
using EventCalendar.Domain.Models;

namespace EventCalendar.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IUserService
{
    public async Task<User> Register(string login, string password)
    {
        var existingUser = await userRepository.FindUserAsync(login);
        if (existingUser != null)
            throw new BadRequestException("User with this login already exists");

        var hashedPassword = passwordHasher.Hash(password);
        var user = User.MakeNew(login, hashedPassword, Role.User);
        await userRepository.CreateUserAsync(user);
        await userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<string> Login(string login, string password)
    {
        var user = await userRepository.FindUserAsync(login);
        if (user == null)
            throw new NotFoundException("User with this login does not exist");

        if (!passwordHasher.CheckPassword(password, user.PasswordHash))
            throw new BadRequestException("Invalid password");

        return tokenService.GenerateToken(user.Id, login, user.Role);
    }
}
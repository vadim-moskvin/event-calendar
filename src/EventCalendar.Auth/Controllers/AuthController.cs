using EventCalendar.Auth.Application.Services;
using EventCalendar.Auth.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Auth.Controllers;

/// <summary>
/// Эндпоинты для аутентификации
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="registerDto">Информация о пользователе в виде Json-объекта</param>
    /// <response code="201">Пользователь зарегистрирован</response>
    /// <response code="409">Пользователь с таким логином существует</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Consumes("application/json")]
    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        await userService.Register(registerDto.Login, registerDto.Password, registerDto.Role);
        return Created();
    }

    /// <summary>
    /// Получение JWT-токена по логину и паролю
    /// </summary>
    /// <param name="loginDto">Информация о пользователе в виде Json-объекта</param>
    /// <response code="200">Токен успешно создан</response>
    /// <response code="409">Пользователь с таким логином существует</response>
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    [Produces("application/json")]
    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        var token = await userService.Login(loginDto.Login, loginDto.Password);
        return Ok(new TokenDto { Token = token });
    }
}
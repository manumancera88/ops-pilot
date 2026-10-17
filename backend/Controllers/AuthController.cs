using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

[HttpPost("register")]
public async Task<ActionResult<UserDto>> Register(
    RegisterCompanyDto dto)
{
    var result = await _authService.RegisterAsync(dto);

    if (result.User is null)
    {
        return BadRequest(result.Error);
    }

    return Ok(result.User);
}


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result.Response is null)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Response);
    }
}
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthService(
        AppDbContext context,
        PasswordService passwordService,
        JwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<(UserDto? User, string? Error)> RegisterAsync(
        RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return (null, "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return (null, "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return (null, "Password is required.");
        }

        if (dto.Password.Length < 6)
        {
            return (null, "Password must have at least 6 characters.");
        }

        var companyExists = await _context.Companies
            .AnyAsync(company => company.Id == dto.CompanyId);

        if (!companyExists)
        {
            return (null, "The specified company does not exist.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(user => user.Email == normalizedEmail);

        if (emailExists)
        {
            return (null, "A user with this email already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(dto.Password),
            CompanyId = dto.CompanyId
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return (
            new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId
            },
            null
        );
    }


 public async Task<(AuthResponseDto? Response, string? Error)> LoginAsync(
        LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return (null, "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return (null, "Password is required.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail);

        if (user is null)
        {
            return (null, "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return (null, "User account is inactive.");
        }

        var passwordIsValid = _passwordService.VerifyPassword(
            dto.Password,
            user.PasswordHash);

        if (!passwordIsValid)
        {
            return (null, "Invalid email or password.");
        }

        var (token, expiresAt) = _jwtService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId
            }
        };

        return (response, null);
    }
}
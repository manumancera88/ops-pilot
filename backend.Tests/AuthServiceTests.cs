
using System.IdentityModel.Tokens.Jwt;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace backend.Tests;

public class AuthServiceTests
{
    private const string TestPassword = "SenhaTeste123!";
    private const string JwtTestKey =
        "OpsPilot_Test_Key_2026_AtLeast_32_Characters_Long!";

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AuthTests_{Guid.NewGuid()}")
            .ConfigureWarnings(warnings =>
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static AuthService CreateService(AppDbContext context)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = JwtTestKey
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var passwordService = new PasswordService();
        var jwtService = new JwtService(configuration);

        return new AuthService(context, passwordService, jwtService);
    }

    private static RegisterCompanyDto ValidRegistration(
        string email = "user@example.com")
    {
        return new RegisterCompanyDto
        {
            CompanyName = "Empresa Teste",
            TaxId = "TEST-001",
            Name = "Usuario Teste",
            Email = email,
            Password = TestPassword
        };
    }

    private static async Task SeedUserAsync(
        AppDbContext context,
        bool isActive = true)
    {
        context.Companies.Add(new Company
        {
            Id = 1,
            Name = "Empresa Existente"
        });

        var passwordService = new PasswordService();

        context.Users.Add(new User
        {
            Id = 1,
            Name = "Usuario Existente",
            Email = "existente@example.com",
            PasswordHash = passwordService.HashPassword(TestPassword),
            CompanyId = 1,
            IsActive = isActive
        });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterAsync_CreatesCompanyAndUser()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var (user, error) = await service.RegisterAsync(
            ValidRegistration());

        Assert.Null(error);
        Assert.NotNull(user);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("Usuario Teste", user.Name);

        var company = await context.Companies
            .SingleAsync(item => item.Name == "Empresa Teste");

        var savedUser = await context.Users
            .SingleAsync(item => item.Email == "user@example.com");

        Assert.Equal(company.Id, user.CompanyId);
        Assert.Equal(company.Id, savedUser.CompanyId);
        Assert.Equal(1, await context.Companies.CountAsync());
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmail()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = ValidRegistration("  USER@EXAMPLE.COM  ");

        var (user, error) = await service.RegisterAsync(dto);

        Assert.Null(error);
        Assert.NotNull(user);
        Assert.Equal("user@example.com", user.Email);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail()
    {
        using var context = CreateContext();
        await SeedUserAsync(context);

        var service = CreateService(context);
        var dto = ValidRegistration(" EXISTENTE@example.com ");

        var (user, error) = await service.RegisterAsync(dto);

        Assert.Null(user);
        Assert.Equal("A user with this email already exists.", error);
        Assert.Single(await context.Companies.ToListAsync());
        Assert.Single(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task RegisterAsync_RejectsPasswordShorterThanSixCharacters()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = ValidRegistration();
        dto.Password = "12345";

        var (user, error) = await service.RegisterAsync(dto);

        Assert.Null(user);
        Assert.Equal(
            "Password must have at least 6 characters.",
            error);
        Assert.Empty(await context.Companies.ToListAsync());
        Assert.Empty(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task RegisterAsync_StoresPasswordAsHash()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var (user, error) = await service.RegisterAsync(
            ValidRegistration());

        Assert.Null(error);
        Assert.NotNull(user);

        var savedUser = await context.Users
            .SingleAsync(item => item.Email == "user@example.com");

        Assert.NotEqual(TestPassword, savedUser.PasswordHash);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                TestPassword,
                savedUser.PasswordHash));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenForValidCredentials()
    {
        using var context = CreateContext();
        await SeedUserAsync(context);

        var service = CreateService(context);

        var (response, error) = await service.LoginAsync(
            new LoginDto
            {
                Email = " EXISTENTE@example.com ",
                Password = TestPassword
            });

        Assert.Null(error);
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("existente@example.com", response.User.Email);
        Assert.Equal(1, response.User.CompanyId);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(response.Token);

        Assert.Equal("1", token.Claims.Single(
            claim => claim.Type == "companyId").Value);

        Assert.Equal("1", token.Claims.Single(
            claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public async Task LoginAsync_RejectsIncorrectPassword()
    {
        using var context = CreateContext();
        await SeedUserAsync(context);

        var service = CreateService(context);

        var (response, error) = await service.LoginAsync(
            new LoginDto
            {
                Email = "existente@example.com",
                Password = "SenhaErrada123!"
            });

        Assert.Null(response);
        Assert.Equal("Invalid email or password.", error);
    }

    [Fact]
    public async Task LoginAsync_RejectsInactiveUser()
    {
        using var context = CreateContext();
        await SeedUserAsync(context, isActive: false);

        var service = CreateService(context);

        var (response, error) = await service.LoginAsync(
            new LoginDto
            {
                Email = "existente@example.com",
                Password = TestPassword
            });

        Assert.Null(response);
        Assert.Equal("User account is inactive.", error);
    }
}

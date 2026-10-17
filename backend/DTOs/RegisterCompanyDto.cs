
namespace backend.DTOs;

public class RegisterCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

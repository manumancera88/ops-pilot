namespace backend.DTOs;

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }
}
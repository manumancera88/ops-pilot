namespace backend.DTOs;

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public bool IsActive { get; set; }
}

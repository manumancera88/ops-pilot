namespace backend.DTOs;

public class CompanyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}
namespace backend.DTOs;

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int CompanyId { get; set; }
}
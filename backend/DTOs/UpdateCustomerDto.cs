namespace backend.DTOs;

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}
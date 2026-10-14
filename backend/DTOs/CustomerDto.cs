namespace backend.DTOs;

public class CustomerDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int CompanyId { get; set; }
}
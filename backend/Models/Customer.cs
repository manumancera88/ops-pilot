namespace backend.Models;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
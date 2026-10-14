namespace backend.Models;

public class Appointment
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Notes { get; set; }

    public bool IsCancelled { get; set; } = false;

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;
}
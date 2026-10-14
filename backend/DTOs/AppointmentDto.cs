namespace backend.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }
    public int CompanyId { get; set; }
    public int CustomerId { get; set; }
}
namespace backend.DTOs;

public class UpdateAppointmentDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }
}
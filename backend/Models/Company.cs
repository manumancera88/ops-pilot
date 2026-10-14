namespace backend.Models;

public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<User> Users { get; set; } = new List<User>();
}

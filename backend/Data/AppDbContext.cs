using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasOne(customer => customer.Company)
            .WithMany(company => company.Customers)
            .HasForeignKey(customer => customer.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Company)
            .WithMany(company => company.Appointments)
            .HasForeignKey(appointment => appointment.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Customer)
            .WithMany(customer => customer.Appointments)
            .HasForeignKey(appointment => appointment.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Company)
            .WithMany(company => company.Users)
            .HasForeignKey(user => user.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);    
    }
}
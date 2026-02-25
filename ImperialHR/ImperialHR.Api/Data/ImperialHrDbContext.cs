using ImperialHR.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ImperialHR.Api.Data;

public class ImperialHrDbContext : DbContext
{
    public ImperialHrDbContext(DbContextOptions<ImperialHrDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Request> Requests => Set<Request>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employees: самозв’язок Manager
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Requests
        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("Requests");

            entity.Property(r => r.Comment)
                  .HasColumnType("nvarchar(max)");

            // EmployeeId -> Employee
            entity.HasOne(r => r.Employee)
                  .WithMany() // НЕ вимагає Employee.Requests
                  .HasForeignKey(r => r.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ApproverId -> Approver
            entity.HasOne(r => r.Approver)
                  .WithMany() // НЕ вимагає Employee.Approvals
                  .HasForeignKey(r => r.ApproverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
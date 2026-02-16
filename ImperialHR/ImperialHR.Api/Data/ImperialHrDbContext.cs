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

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(m => m.Subordinates)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

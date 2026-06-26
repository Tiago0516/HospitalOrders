using HospitalOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalOrders.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.PatientId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(o => o.PatientName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(o => o.ServiceCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(o => o.ServiceDescription)
                .HasMaxLength(500);

            entity.Property(o => o.Priority)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(o => o.FailureReason)
                .HasMaxLength(1000);

            entity.HasIndex(o => o.PatientId);
            entity.HasIndex(o => o.Status);
        });
    }
}

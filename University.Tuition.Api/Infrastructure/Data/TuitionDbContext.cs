using Microsoft.EntityFrameworkCore;
using University.Tuition.Api.Domain.Entities;

namespace University.Tuition.Api.Infrastructure.Data;

public class TuitionDbContext : DbContext
{
    public TuitionDbContext(DbContextOptions<TuitionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<TuitionCharge> TuitionCharges => Set<TuitionCharge>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // STUDENTNO unique
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentNo)
            .IsUnique();

        // Tuples: (StudentId, Term) - hızlı sorgu için
        modelBuilder.Entity<TuitionCharge>()
            .HasIndex(c => new { c.StudentId, c.Term });

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.StudentId, p.Term });

        // decimal precision
        modelBuilder.Entity<TuitionCharge>()
            .Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Payment>()
            .Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        // ---- Seed (örnek veriler) ----
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, StudentNo = "20201234", FullName = "Selin Yılmaz" },
            new Student { Id = 2, StudentNo = "20204567", FullName = "Ahmet Demir" }
        );

        modelBuilder.Entity<TuitionCharge>().HasData(
            new TuitionCharge { Id = 1, StudentId = 1, Term = "2025-Spring", Amount = 12000.00m, CreatedAt = DateTime.UtcNow },
            new TuitionCharge { Id = 2, StudentId = 2, Term = "2025-Spring", Amount = 9000.00m, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Payment>().HasData(
            new Payment { Id = 1, StudentId = 1, Term = "2025-Spring", Amount = 3000.00m, CreatedAt = DateTime.UtcNow }
        );

    }
}

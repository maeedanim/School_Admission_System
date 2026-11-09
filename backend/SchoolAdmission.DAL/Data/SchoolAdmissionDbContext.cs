using Microsoft.EntityFrameworkCore;
using SchoolAdmission.DAL.Entities;

namespace SchoolAdmission.DAL.Data;

public class SchoolAdmissionDbContext : DbContext
{
    public SchoolAdmissionDbContext(
        DbContextOptions<SchoolAdmissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdmissionClass> Classes => Set<AdmissionClass>();

    public DbSet<AdmissionHold> Holds => Set<AdmissionHold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Admission Classes
        modelBuilder.Entity<AdmissionClass>(entity =>
        {
            entity.ToTable("classes", table =>
            {
                table.HasCheckConstraint(
                    "ck_classes_total_seats_positive",
                    "total_seats > 0");
            });

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(c => c.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.TotalSeats)
                .HasColumnName("total_seats")
                .IsRequired();

            entity.Property(c => c.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.HasMany(c => c.Holds)
                .WithOne(h => h.Class)
                .HasForeignKey(h => h.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Admission Holds
        modelBuilder.Entity<AdmissionHold>(entity =>
        {
            entity.ToTable("holds", table =>
            {
                table.HasCheckConstraint(
                    "ck_holds_student_id_format",
                    "student_id ~ '^[0-9]{17}$'");

                table.HasCheckConstraint(
                    "ck_holds_status",
                    "status IN ('HELD', 'CONFIRMED', 'EXPIRED', 'CANCELLED')");
            });

            entity.HasKey(h => h.Id);

            entity.Property(h => h.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(h => h.ClassId)
                .HasColumnName("class_id")
                .IsRequired();

            entity.Property(h => h.StudentId)
                .HasColumnName("student_id")
                .HasMaxLength(17)
                .IsRequired();

            entity.Property(h => h.Status)
                .HasColumnName("status")
                .HasConversion(
                    status => status.ToString().ToUpperInvariant(),
                    value => Enum.Parse<SchoolAdmission.BLL.Enums.AdmissionHoldStatus>(
                        value,
                        true))
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(h => h.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(h => h.ExpiresAtUtc)
                .HasColumnName("expires_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(h => h.ConfirmedAtUtc)
                .HasColumnName("confirmed_at")
                .HasColumnType("timestamp with time zone");

            entity.Property(h => h.CancelledAtUtc)
                .HasColumnName("cancelled_at")
                .HasColumnType("timestamp with time zone");

            // Index for class seat-count queries
            entity.HasIndex(h => new
            {
                h.ClassId,
                h.Status,
                h.ExpiresAtUtc
            })
                .HasDatabaseName("ix_holds_class_status_expiry");

            // Index for a student's admission history
            entity.HasIndex(h => new
            {
                h.StudentId,
                h.CreatedAtUtc
            })
                .HasDatabaseName("ix_holds_student_created");

            // One active application per student per class
            entity.HasIndex(h => new
            {
                h.ClassId,
                h.StudentId
            })
                .IsUnique()
                .HasFilter("status IN ('HELD', 'CONFIRMED')")
                .HasDatabaseName("ux_holds_class_student_active");
        });
    }
}
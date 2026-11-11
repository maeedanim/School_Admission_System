
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.DAL.Entities;
using SchoolAdmission.DAL.Entities.Identity;

namespace SchoolAdmission.DAL.Data;

public class SchoolAdmissionDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public SchoolAdmissionDbContext(
        DbContextOptions<SchoolAdmissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdmissionClass> Classes => Set<AdmissionClass>();

    public DbSet<AdmissionHold> Holds => Set<AdmissionHold>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Application User Configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.BirthRegistrationNumber)
                .HasMaxLength(17);

            entity.Property(u => u.IsActive)
                .HasDefaultValue(true);

            entity.Property(u => u.CreatedAtUtc)
                .HasColumnType("timestamp with time zone");

            // Birth Registration Number must be unique when provided.
            // Admin accounts can have NULL values.
            entity.HasIndex(u => u.BirthRegistrationNumber)
                .IsUnique()
                .HasFilter("\"BirthRegistrationNumber\" IS NOT NULL");
        });

        // Refresh Token Configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(t => t.ReplacedByTokenHash)
                .HasMaxLength(64);

            entity.Property(t => t.CreatedAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.Property(t => t.ExpiresAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.Property(t => t.RevokedAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(t => t.TokenHash)
                .IsUnique();

            entity.HasIndex(t => t.UserId);

            entity.HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

            // Explicitly map the nullable user foreign key to snake_case.
            entity.Property(h => h.UserId)
                .HasColumnName("user_id");

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

            // Student account relationship.
            // Nullable to preserve existing admission records.
            entity.HasOne(h => h.User)
                .WithMany(u => u.Holds)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(h => h.UserId);

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

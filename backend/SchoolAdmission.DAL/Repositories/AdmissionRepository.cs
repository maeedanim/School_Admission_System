
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.BLL.Enums;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Entities;

namespace SchoolAdmission.DAL.Repositories;

public class AdmissionRepository : IAdmissionRepository
{
    private readonly SchoolAdmissionDbContext _context;

    public AdmissionRepository(SchoolAdmissionDbContext context)
    {
        _context = context;
    }

    // Class operations

    public async Task<CreatedClassData> CreateClassAsync(
        string name,
        int totalSeats,
        CancellationToken cancellationToken = default)
    {
        var admissionClass = new AdmissionClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            TotalSeats = totalSeats,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Classes.Add(admissionClass);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreatedClassData
        {
            Id = admissionClass.Id,
            Name = admissionClass.Name,
            TotalSeats = admissionClass.TotalSeats
        };
    }

    public async Task<IReadOnlyList<ClassData>> GetClassesAsync(
        CancellationToken cancellationToken = default)
    {
        DateTime currentTime = await _context.Database
            .SqlQuery<DateTime>(
                $"SELECT clock_timestamp() AS \"Value\"")
            .SingleAsync(cancellationToken);

        return await _context.Classes
            .AsNoTracking()
            .Select(admissionClass => new ClassData
            {
                Id = admissionClass.Id,
                Name = admissionClass.Name,
                TotalSeats = admissionClass.TotalSeats,

                HeldSeats = admissionClass.Holds.Count(hold =>
                    hold.Status == AdmissionHoldStatus.Held &&
                    hold.ExpiresAtUtc > currentTime),

                ConfirmedSeats = admissionClass.Holds.Count(hold =>
                    hold.Status == AdmissionHoldStatus.Confirmed),

                AvailableSeats = admissionClass.TotalSeats
                    - admissionClass.Holds.Count(hold =>
                        hold.Status == AdmissionHoldStatus.Held &&
                        hold.ExpiresAtUtc > currentTime)
                    - admissionClass.Holds.Count(hold =>
                        hold.Status == AdmissionHoldStatus.Confirmed)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassData?> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        DateTime currentTime = await _context.Database
            .SqlQuery<DateTime>(
                $"SELECT clock_timestamp() AS \"Value\"")
            .SingleAsync(cancellationToken);

        return await _context.Classes
            .AsNoTracking()
            .Where(admissionClass => admissionClass.Id == classId)
            .Select(admissionClass => new ClassData
            {
                Id = admissionClass.Id,
                Name = admissionClass.Name,
                TotalSeats = admissionClass.TotalSeats,

                HeldSeats = admissionClass.Holds.Count(hold =>
                    hold.Status == AdmissionHoldStatus.Held &&
                    hold.ExpiresAtUtc > currentTime),

                ConfirmedSeats = admissionClass.Holds.Count(hold =>
                    hold.Status == AdmissionHoldStatus.Confirmed),

                AvailableSeats = admissionClass.TotalSeats
                    - admissionClass.Holds.Count(hold =>
                        hold.Status == AdmissionHoldStatus.Held &&
                        hold.ExpiresAtUtc > currentTime)
                    - admissionClass.Holds.Count(hold =>
                        hold.Status == AdmissionHoldStatus.Confirmed)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Hold creation

    public async Task<HoldCreationResult> CreateHoldAsync(
        Guid classId,
        string studentId,
        int holdDurationSeconds,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        // Lock the class row to serialize hold creation for this class.
        var admissionClass = await _context.Classes
            .FromSqlInterpolated(
                $"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (admissionClass is null)
        {
            await transaction.CommitAsync(cancellationToken);

            return new HoldCreationResult
            {
                Status = HoldCreationStatus.ClassNotFound
            };
        }

        // Obtain the authoritative database timestamp after acquiring the lock.
        DateTime databaseNow = await _context.Database
            .SqlQuery<DateTime>(
                $"SELECT clock_timestamp() AS \"Value\"")
            .SingleAsync(cancellationToken);

        // Expire outdated holds for this class.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'EXPIRED'
            WHERE class_id = {classId}
              AND status = 'HELD'
              AND expires_at <= {databaseNow}
            """,
            cancellationToken);

        // Prevent duplicate active applications by the same student.
        bool alreadyApplied = await _context.Holds.AnyAsync(
            hold =>
                hold.ClassId == classId &&
                hold.StudentId == studentId &&
                (hold.Status == AdmissionHoldStatus.Held ||
                 hold.Status == AdmissionHoldStatus.Confirmed),
            cancellationToken);

        if (alreadyApplied)
        {
            await transaction.CommitAsync(cancellationToken);

            return new HoldCreationResult
            {
                Status = HoldCreationStatus.AlreadyApplied
            };
        }

        // Count occupied seats after expiring outdated holds.
        int occupiedSeats = await _context.Holds.CountAsync(
            hold =>
                hold.ClassId == classId &&
                (hold.Status == AdmissionHoldStatus.Held ||
                 hold.Status == AdmissionHoldStatus.Confirmed),
            cancellationToken);

        if (occupiedSeats >= admissionClass.TotalSeats)
        {
            await transaction.CommitAsync(cancellationToken);

            return new HoldCreationResult
            {
                Status = HoldCreationStatus.SeatsUnavailable
            };
        }

        // Create the new hold using database time.
        var hold = new AdmissionHold
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            StudentId = studentId,
            Status = AdmissionHoldStatus.Held,
            CreatedAtUtc = databaseNow,
            ExpiresAtUtc = databaseNow.AddSeconds(holdDurationSeconds)
        };

        _context.Holds.Add(hold);

        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new HoldCreationResult
        {
            Status = HoldCreationStatus.Created,
            Hold = new HoldData
            {
                Id = hold.Id,
                ClassId = hold.ClassId,
                StudentId = hold.StudentId,
                Status = hold.Status,
                CreatedAtUtc = hold.CreatedAtUtc,
                ExpiresAtUtc = hold.ExpiresAtUtc,
                ConfirmedAtUtc = hold.ConfirmedAtUtc,
                CancelledAtUtc = hold.CancelledAtUtc
            }
        };
    }

    // Hold retrieval

    public async Task<HoldData?> GetHoldByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        // Persist expiry when the hold has passed its expiration time.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'EXPIRED'
            WHERE id = {holdId}
              AND status = 'HELD'
              AND expires_at <= clock_timestamp()
            """,
            cancellationToken);

        return await _context.Holds
            .AsNoTracking()
            .Where(hold => hold.Id == holdId)
            .Select(hold => new HoldData
            {
                Id = hold.Id,
                ClassId = hold.ClassId,
                StudentId = hold.StudentId,
                Status = hold.Status,
                CreatedAtUtc = hold.CreatedAtUtc,
                ExpiresAtUtc = hold.ExpiresAtUtc,
                ConfirmedAtUtc = hold.ConfirmedAtUtc,
                CancelledAtUtc = hold.CancelledAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    // Hold confirmation

    public async Task<HoldConfirmationStatus> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        // Find the associated class before acquiring locks.
        Guid? classId = await _context.Holds
            .AsNoTracking()
            .Where(hold => hold.Id == holdId)
            .Select(hold => (Guid?)hold.ClassId)
            .SingleOrDefaultAsync(cancellationToken);

        if (classId is null)
        {
            return HoldConfirmationStatus.HoldNotFound;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        // Always acquire the class lock first.
        var admissionClass = await _context.Classes
            .FromSqlInterpolated(
                $"SELECT * FROM classes WHERE id = {classId.Value} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (admissionClass is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.HoldNotFound;
        }

        // Acquire the hold lock second.
        var hold = await _context.Holds
            .FromSqlInterpolated(
                $"SELECT * FROM holds WHERE id = {holdId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (hold is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.HoldNotFound;
        }

        // Verify ownership.
        if (hold.StudentId != studentId)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.NotOwner;
        }

        // Handle previously completed states.
        if (hold.Status == AdmissionHoldStatus.Confirmed)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.AlreadyConfirmed;
        }

        if (hold.Status == AdmissionHoldStatus.Expired)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.Expired;
        }

        if (hold.Status == AdmissionHoldStatus.Cancelled)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.NotActive;
        }

        // Confirm only if the hold is still active.
        int updatedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'CONFIRMED',
                confirmed_at = clock_timestamp()
            WHERE id = {holdId}
              AND student_id = {studentId}
              AND status = 'HELD'
              AND expires_at > clock_timestamp()
            """,
            cancellationToken);

        if (updatedRows == 1)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldConfirmationStatus.Confirmed;
        }

        // Persist expiration if the hold expired during processing.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'EXPIRED'
            WHERE id = {holdId}
              AND status = 'HELD'
              AND expires_at <= clock_timestamp()
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return HoldConfirmationStatus.NotActive;
    }

    // Hold cancellation

    public async Task<HoldCancellationStatus> CancelHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        // Find the associated class before acquiring locks.
        Guid? classId = await _context.Holds
            .AsNoTracking()
            .Where(hold => hold.Id == holdId)
            .Select(hold => (Guid?)hold.ClassId)
            .SingleOrDefaultAsync(cancellationToken);

        if (classId is null)
        {
            return HoldCancellationStatus.HoldNotFound;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        // Always acquire the class lock first.
        var admissionClass = await _context.Classes
            .FromSqlInterpolated(
                $"SELECT * FROM classes WHERE id = {classId.Value} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (admissionClass is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.HoldNotFound;
        }

        // Acquire the hold lock second.
        var hold = await _context.Holds
            .FromSqlInterpolated(
                $"SELECT * FROM holds WHERE id = {holdId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (hold is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.HoldNotFound;
        }

        // Verify ownership.
        if (hold.StudentId != studentId)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.NotOwner;
        }

        if (hold.Status == AdmissionHoldStatus.Cancelled)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.AlreadyCancelled;
        }

        if (hold.Status != AdmissionHoldStatus.Held)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.NotActive;
        }

        // Cancel only if the hold has not expired.
        int updatedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'CANCELLED',
                cancelled_at = clock_timestamp()
            WHERE id = {holdId}
              AND student_id = {studentId}
              AND status = 'HELD'
              AND expires_at > clock_timestamp()
            """,
            cancellationToken);

        if (updatedRows == 1)
        {
            await transaction.CommitAsync(cancellationToken);
            return HoldCancellationStatus.Cancelled;
        }

        // Persist expiration if the hold has expired.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'EXPIRED'
            WHERE id = {holdId}
              AND status = 'HELD'
              AND expires_at <= clock_timestamp()
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return HoldCancellationStatus.NotActive;
    }

    // Student hold history

    public async Task<IReadOnlyList<MyHoldData>> GetStudentHoldsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        // Persist expiry for this student's outdated holds.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE holds
            SET status = 'EXPIRED'
            WHERE student_id = {studentId}
              AND status = 'HELD'
              AND expires_at <= clock_timestamp()
            """,
            cancellationToken);

        return await _context.Holds
            .AsNoTracking()
            .Where(hold => hold.StudentId == studentId)
            .OrderByDescending(hold => hold.CreatedAtUtc)
            .Select(hold => new MyHoldData
            {
                HoldId = hold.Id,
                ClassId = hold.ClassId,
                ClassName = hold.Class.Name,
                Status = hold.Status,
                ExpiresAtUtc = hold.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}

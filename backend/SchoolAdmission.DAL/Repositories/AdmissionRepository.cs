
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
        var currentTime = DateTime.UtcNow;

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
        var currentTime = DateTime.UtcNow;

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

    // Temporary placeholders.
    // Replace these before considering the repository complete.

    public Task<HoldCreationResult> CreateHoldAsync(
        Guid classId,
        string studentId,
        int holdDurationSeconds,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<HoldData?> GetHoldByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<HoldConfirmationStatus> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<HoldCancellationStatus> CancelHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<MyHoldData>> GetStudentHoldsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

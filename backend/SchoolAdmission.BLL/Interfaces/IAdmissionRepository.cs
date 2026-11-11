using SchoolAdmission.BLL.Enums;
using SchoolAdmission.BLL.Models;

namespace SchoolAdmission.BLL.Interfaces;

public interface IAdmissionRepository
{
    // Class operations
    Task<CreatedClassData> CreateClassAsync(
        string name,
        int totalSeats,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassData>> GetClassesAsync(
        CancellationToken cancellationToken = default);

    Task<ClassData?> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    // Hold operations
    Task<HoldCreationResult> CreateHoldAsync(
        Guid classId,
        Guid userId,
        string studentId,
        int holdDurationSeconds,
        CancellationToken cancellationToken = default);

    Task<HoldData?> GetHoldByIdAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<HoldConfirmationStatus> ConfirmHoldAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<HoldCancellationStatus> CancelHoldAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyHoldData>> GetStudentHoldsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

Task<IReadOnlyList<AdminApplicationData>> GetAllApplicationsAsync(
    CancellationToken cancellationToken = default);
}

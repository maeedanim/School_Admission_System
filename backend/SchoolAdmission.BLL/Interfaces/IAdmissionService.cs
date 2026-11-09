using SchoolAdmission.BLL.DTOs.Requests;
using SchoolAdmission.BLL.DTOs.Responses;

namespace SchoolAdmission.BLL.Interfaces;

public interface IAdmissionService
{
    // Class operations
    Task<CreateClassResponse> CreateClassAsync(
        CreateClassRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassResponse>> GetClassesAsync(
        CancellationToken cancellationToken = default);

    Task<ClassResponse> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    // Hold operations
    Task<CreateHoldResponse> CreateHoldAsync(
        Guid classId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<HoldResponse> GetHoldByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<ConfirmHoldResponse> ConfirmHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task CancelHoldAsync(
        Guid holdId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyHoldResponse>> GetStudentHoldsAsync(
        string studentId,
        CancellationToken cancellationToken = default);
}
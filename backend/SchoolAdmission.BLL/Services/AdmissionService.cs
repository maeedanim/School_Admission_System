
using System.Globalization;
using SchoolAdmission.BLL.DTOs.Requests;
using SchoolAdmission.BLL.DTOs.Responses;
using SchoolAdmission.BLL.Enums;
using SchoolAdmission.BLL.Exceptions;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models;
using SchoolAdmission.BLL.Models.Auth;
using Microsoft.Extensions.Configuration;

namespace SchoolAdmission.BLL.Services;

public class AdmissionService : IAdmissionService
{
    private readonly IAdmissionRepository _repository;
    private readonly IAuthService _authService;
    private readonly int _holdDurationSeconds;

    public AdmissionService(
        IAdmissionRepository repository,
        IAuthService authService,
        IConfiguration configuration)
    {
        _repository = repository;
        _authService = authService;

        string? configuredDuration = configuration["HOLD_DURATION_SECONDS"];

        if (string.IsNullOrWhiteSpace(configuredDuration))
        {
            _holdDurationSeconds = 600;
        }
        else if (int.TryParse(
            configuredDuration,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out int duration) && duration > 0)
        {
            _holdDurationSeconds = duration;
        }
        else
        {
            throw new InvalidOperationException(
                "HOLD_DURATION_SECONDS must be a positive integer.");
        }
    }

    // Class operations

    public async Task<CreateClassResponse> CreateClassAsync(
        CreateClassRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new AdmissionException(
                "VALIDATION_ERROR",
                "Class information is required.");
        }

        string name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            throw new AdmissionException(
                "VALIDATION_ERROR",
                "Class name must be between 1 and 200 characters.");
        }

        if (request.TotalSeats <= 0)
        {
            throw new AdmissionException(
                "VALIDATION_ERROR",
                "Total seats must be greater than zero.");
        }

        CreatedClassData createdClass = await _repository.CreateClassAsync(
            name,
            request.TotalSeats,
            cancellationToken);

        return new CreateClassResponse
        {
            Id = createdClass.Id,
            Name = createdClass.Name,
            TotalSeats = createdClass.TotalSeats
        };
    }

    public async Task<IReadOnlyList<ClassResponse>> GetClassesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ClassData> classes =
            await _repository.GetClassesAsync(cancellationToken);

        return classes.Select(MapClassResponse).ToList();
    }

    public async Task<ClassResponse> GetClassByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        ClassData? classData =
            await _repository.GetClassByIdAsync(classId, cancellationToken);

        if (classData is null)
        {
            throw new AdmissionException(
                "NOT_FOUND",
                "Class not found.");
        }

        return MapClassResponse(classData);
    }

    // Hold operations

    public async Task<CreateHoldResponse> CreateHoldAsync(
        Guid classId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        AuthUserData user = await GetActiveStudentAsync(
            userId,
            cancellationToken);

        string studentId = user.BirthRegistrationNumber ?? string.Empty;
        ValidateStudentId(studentId);

        HoldCreationResult result = await _repository.CreateHoldAsync(
            classId,
            user.Id,
            studentId,
            _holdDurationSeconds,
            cancellationToken);

        switch (result.Status)
        {
            case HoldCreationStatus.Created:
                if (result.Hold is null)
                {
                    throw new InvalidOperationException(
                        "The repository returned a successful hold creation without hold data.");
                }

                return new CreateHoldResponse
                {
                    HoldId = result.Hold.Id,
                    ClassId = result.Hold.ClassId,
                    StudentId = result.Hold.StudentId,
                    Status = "HELD",
                    ExpiresAt = result.Hold.ExpiresAtUtc
                };

            case HoldCreationStatus.ClassNotFound:
                throw new AdmissionException(
                    "NOT_FOUND",
                    "Class not found.");

            case HoldCreationStatus.SeatsUnavailable:
                throw new AdmissionException(
                    "SEATS_UNAVAILABLE",
                    "No seats are currently available.");

            case HoldCreationStatus.AlreadyApplied:
                throw new AdmissionException(
                    "ALREADY_APPLIED",
                    "You already have an active application for this class.");

            default:
                throw new InvalidOperationException(
                    "Unknown hold creation result.");
        }
    }

    public async Task<HoldResponse> GetHoldByIdAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await GetActiveStudentAsync(userId, cancellationToken);

        HoldData? hold = await _repository.GetHoldByIdAsync(
            holdId,
            userId,
            cancellationToken);

        if (hold is null)
        {
            throw new AdmissionException(
                "NOT_FOUND",
                "Hold not found.");
        }

        return new HoldResponse
        {
            HoldId = hold.Id,
            ClassId = hold.ClassId,
            Status = hold.Status.ToString().ToUpperInvariant(),
            ExpiresAt = hold.ExpiresAtUtc
        };
    }

    public async Task<ConfirmHoldResponse> ConfirmHoldAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await GetActiveStudentAsync(userId, cancellationToken);

        HoldConfirmationStatus result =
            await _repository.ConfirmHoldAsync(
                holdId,
                userId,
                cancellationToken);

        switch (result)
        {
            case HoldConfirmationStatus.Confirmed:
            case HoldConfirmationStatus.AlreadyConfirmed:
                return new ConfirmHoldResponse
                {
                    HoldId = holdId,
                    Status = "CONFIRMED"
                };

            case HoldConfirmationStatus.HoldNotFound:
                throw new AdmissionException(
                    "NOT_FOUND",
                    "Hold not found.");

            case HoldConfirmationStatus.NotOwner:
                throw new AdmissionException(
                    "NOT_OWNER",
                    "This hold does not belong to you.");

            case HoldConfirmationStatus.Expired:
                throw new AdmissionException(
                    "HOLD_EXPIRED",
                    "This hold has expired.");

            case HoldConfirmationStatus.NotActive:
                throw new AdmissionException(
                    "HOLD_NOT_ACTIVE",
                    "This hold is no longer active.");

            default:
                throw new InvalidOperationException(
                    "Unknown hold confirmation result.");
        }
    }

    public async Task CancelHoldAsync(
        Guid holdId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await GetActiveStudentAsync(userId, cancellationToken);

        HoldCancellationStatus result =
            await _repository.CancelHoldAsync(
                holdId,
                userId,
                cancellationToken);

        switch (result)
        {
            case HoldCancellationStatus.Cancelled:
            case HoldCancellationStatus.AlreadyCancelled:
                return;

            case HoldCancellationStatus.HoldNotFound:
                throw new AdmissionException(
                    "NOT_FOUND",
                    "Hold not found.");

            case HoldCancellationStatus.NotOwner:
                throw new AdmissionException(
                    "NOT_OWNER",
                    "This hold does not belong to you.");

            case HoldCancellationStatus.NotActive:
                throw new AdmissionException(
                    "HOLD_NOT_ACTIVE",
                    "This hold cannot be cancelled because it is no longer active.");

            default:
                throw new InvalidOperationException(
                    "Unknown hold cancellation result.");
        }
    }

    public async Task<IReadOnlyList<MyHoldResponse>> GetStudentHoldsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await GetActiveStudentAsync(userId, cancellationToken);

        IReadOnlyList<MyHoldData> holds =
            await _repository.GetStudentHoldsAsync(
                userId,
                cancellationToken);

        return holds.Select(hold => new MyHoldResponse
        {
            HoldId = hold.HoldId,
            ClassId = hold.ClassId,
            ClassName = hold.ClassName,
            Status = hold.Status.ToString().ToUpperInvariant(),
            ExpiresAt = hold.ExpiresAtUtc
        }).ToList();
    }

    // Validation and mapping helpers

    private async Task<AuthUserData> GetActiveStudentAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        AuthUserData? user = await _authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new AdmissionException(
                "UNAUTHENTICATED",
                "Your account is unavailable. Please log in again.");
        }

        if (!user.Roles.Contains(
            "Student",
            StringComparer.OrdinalIgnoreCase))
        {
            throw new AdmissionException(
                "FORBIDDEN",
                "Only students can perform this operation.");
        }

        return user;
    }

    private static void ValidateStudentId(string studentId)
    {
        if (studentId is null ||
            studentId.Length != 17 ||
            studentId.Any(character => character < '0' || character > '9'))
        {
            throw new AdmissionException(
                "INVALID_STUDENT_ID",
                "Student ID must contain exactly 17 digits.");
        }
    }

    private static ClassResponse MapClassResponse(ClassData classData)
    {
        return new ClassResponse
        {
            Id = classData.Id,
            Name = classData.Name,
            TotalSeats = classData.TotalSeats,
            HeldSeats = classData.HeldSeats,
            ConfirmedSeats = classData.ConfirmedSeats,
            AvailableSeats = classData.AvailableSeats
        };
    }
}

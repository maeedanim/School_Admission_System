using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.BLL.DTOs.Requests;
using SchoolAdmission.BLL.Exceptions;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.DAL.Data;



namespace SchoolAdmission.Tests;

public class AdmissionIntegrationTests
    : IClassFixture<PostgresTestFixture>
{
    private readonly PostgresTestFixture _fixture;

    public AdmissionIntegrationTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateClass_ShouldPersistClassAndInitializeSeatCounts()
    {
        // Arrange
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var request = new CreateClassRequest
        {
            Name = $"Integration Test Class {Guid.NewGuid()}",
            TotalSeats = 10
        };

        // Act: Create a class in PostgreSQL.
        var createdClass = await admissionService.CreateClassAsync(request);

        // Act: Retrieve the same class.
        var retrievedClass = await admissionService
            .GetClassByIdAsync(createdClass.Id);

        // Assert: Verify persisted class information.
        Assert.Equal(createdClass.Id, retrievedClass.Id);
        Assert.Equal(request.Name, retrievedClass.Name);
        Assert.Equal(10, retrievedClass.TotalSeats);

        // Assert: Verify initial seat counts.
        Assert.Equal(0, retrievedClass.HeldSeats);
        Assert.Equal(0, retrievedClass.ConfirmedSeats);
        Assert.Equal(10, retrievedClass.AvailableSeats);
    }


    [Fact]
    public async Task CreateHold_WhenClassHasOneSeat_ShouldRejectSecondStudent()
    {
        // Arrange
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Capacity Test {Guid.NewGuid()}",
                TotalSeats = 1
            });

        const string firstStudentId = "11111111111111111";
        const string secondStudentId = "22222222222222222";

        // Act: First student reserves the only available seat.
        var firstHold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            firstStudentId);

        // Assert: First reservation succeeds.
        Assert.Equal("HELD", firstHold.Status);
        Assert.Equal(createdClass.Id, firstHold.ClassId);
        Assert.Equal(firstStudentId, firstHold.StudentId);

        // Act and Assert: Second student must be rejected.
        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.CreateHoldAsync(
                createdClass.Id,
                secondStudentId));

        Assert.Equal("SEATS_UNAVAILABLE", exception.Code);

        // Verify final seat counts.
        var classDetails = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(1, classDetails.HeldSeats);
        Assert.Equal(0, classDetails.ConfirmedSeats);
        Assert.Equal(0, classDetails.AvailableSeats);
    }


    [Fact]
    public async Task CreateHold_WhenStudentAlreadyApplied_ShouldRejectDuplicateApplication()
    {
        // Arrange
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Duplicate Test {Guid.NewGuid()}",
                TotalSeats = 3
            });

        const string studentId = "33333333333333333";

        // Act: Student creates the first application.
        var firstHold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        // Act and Assert: The same student tries to apply again.
        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.CreateHoldAsync(
                createdClass.Id,
                studentId));

        Assert.Equal("ALREADY_APPLIED", exception.Code);

        // Verify that only one seat is occupied.
        var classDetails = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal("HELD", firstHold.Status);
        Assert.Equal(1, classDetails.HeldSeats);
        Assert.Equal(0, classDetails.ConfirmedSeats);
        Assert.Equal(2, classDetails.AvailableSeats);
    }


    [Fact]
    public async Task ConfirmHold_WhenHoldHasExpired_ShouldRejectConfirmationAndReleaseSeat()
    {
        // Arrange
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Expiry Test {Guid.NewGuid()}",
                TotalSeats = 1
            });

        const string studentId = "44444444444444444";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        Assert.Equal("HELD", hold.Status);

        // Force the hold to expire in the test database.
        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolAdmissionDbContext>();

        int updatedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
     UPDATE holds
     SET expires_at = clock_timestamp() - INTERVAL '1 second'
     WHERE id = {hold.HoldId}
     """);

        Assert.Equal(1, updatedRows);

        // Act: Try to confirm without first requesting the hold status.
        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.ConfirmHoldAsync(
                hold.HoldId,
                studentId));

        // Assert: Confirmation must fail because the hold expired.
        Assert.Equal("HOLD_EXPIRED", exception.Code);

        var expiredHold = await admissionService.GetHoldByIdAsync(
    hold.HoldId);

        Assert.Equal("EXPIRED", expiredHold.Status);

        // Verify that the expired hold released its seat.
        var classDetails = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(0, classDetails.HeldSeats);
        Assert.Equal(0, classDetails.ConfirmedSeats);
        Assert.Equal(1, classDetails.AvailableSeats);
    }

    [Fact]
    public async Task ConfirmHold_WhenHoldIsActive_ShouldConfirmAndBeIdempotent()
    {
        using var scope = _fixture.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await service.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Confirmation Test {Guid.NewGuid():N}",
                TotalSeats = 2
            });

        const string studentId = "55555555555555555";

        var hold = await service.CreateHoldAsync(
            createdClass.Id,
            studentId);

        // First confirmation.
        var firstConfirmation = await service.ConfirmHoldAsync(
            hold.HoldId,
            studentId);

        Assert.Equal("CONFIRMED", firstConfirmation.Status);

        // Second confirmation must succeed without double-counting.
        var secondConfirmation = await service.ConfirmHoldAsync(
            hold.HoldId,
            studentId);

        Assert.Equal("CONFIRMED", secondConfirmation.Status);

        // Verify the final seat counts.
        var classData = await service.GetClassByIdAsync(createdClass.Id);

        Assert.Equal(0, classData.HeldSeats);
        Assert.Equal(1, classData.ConfirmedSeats);
        Assert.Equal(1, classData.AvailableSeats);
    }


    [Fact]
    public async Task ConfirmHold_WhenStudentIsNotOwner_ShouldRejectConfirmation()
    {
        using var scope = _fixture.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await service.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Ownership Test {Guid.NewGuid():N}",
                TotalSeats = 2
            });

        const string ownerStudentId = "66666666666666666";
        const string otherStudentId = "77777777777777777";

        var hold = await service.CreateHoldAsync(
            createdClass.Id,
            ownerStudentId);

        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => service.ConfirmHoldAsync(
                hold.HoldId,
                otherStudentId));

        Assert.Equal("NOT_OWNER", exception.Code);

        // The original hold must remain active.
        var classData = await service.GetClassByIdAsync(createdClass.Id);

        Assert.Equal(1, classData.HeldSeats);
        Assert.Equal(0, classData.ConfirmedSeats);
        Assert.Equal(1, classData.AvailableSeats);
    }

    [Fact]
    public async Task CancelHold_WhenHoldIsActive_ShouldCancelAndReleaseSeat()
    {
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Cancellation Test {Guid.NewGuid()}",
                TotalSeats = 2
            });

        const string studentId = "88888888888888888";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        await admissionService.CancelHoldAsync(
            hold.HoldId,
            studentId);

        var cancelledHold = await admissionService.GetHoldByIdAsync(
            hold.HoldId);

        var classStatus = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal("CANCELLED", cancelledHold.Status);
        Assert.Equal(0, classStatus.HeldSeats);
        Assert.Equal(0, classStatus.ConfirmedSeats);
        Assert.Equal(2, classStatus.AvailableSeats);
    }


    [Fact]
    public async Task CancelHold_WhenAlreadyCancelled_ShouldSucceedIdempotently()
    {
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Cancellation Idempotency Test {Guid.NewGuid()}",
                TotalSeats = 2
            });

        const string studentId = "99999999999999999";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        // First cancellation.
        await admissionService.CancelHoldAsync(
            hold.HoldId,
            studentId);

        // Second cancellation should also succeed.
        await admissionService.CancelHoldAsync(
            hold.HoldId,
            studentId);

        var classStatus = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(0, classStatus.HeldSeats);
        Assert.Equal(0, classStatus.ConfirmedSeats);
        Assert.Equal(2, classStatus.AvailableSeats);
    }


    [Fact]
    public async Task CancelHold_WhenStudentIsNotOwner_ShouldRejectCancellation()
    {
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Cancellation Ownership Test {Guid.NewGuid()}",
                TotalSeats = 2
            });

        const string ownerId = "11111111111111111";
        const string otherStudentId = "22222222222222222";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            ownerId);

        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.CancelHoldAsync(
                hold.HoldId,
                otherStudentId));

        Assert.Equal("NOT_OWNER", exception.Code);

        var classStatus = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(1, classStatus.HeldSeats);
        Assert.Equal(0, classStatus.ConfirmedSeats);
        Assert.Equal(1, classStatus.AvailableSeats);
    }


    [Fact]
    public async Task CancelHold_WhenAdmissionIsConfirmed_ShouldRejectCancellation()
    {
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Confirmed Cancellation Test {Guid.NewGuid()}",
                TotalSeats = 2
            });

        const string studentId = "33333333333333333";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        await admissionService.ConfirmHoldAsync(
            hold.HoldId,
            studentId);

        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.CancelHoldAsync(
                hold.HoldId,
                studentId));

        Assert.Equal("HOLD_NOT_ACTIVE", exception.Code);

        var classStatus = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(0, classStatus.HeldSeats);
        Assert.Equal(1, classStatus.ConfirmedSeats);
        Assert.Equal(1, classStatus.AvailableSeats);
    }


    [Fact]
    public async Task CancelHold_WhenHoldHasExpired_ShouldRejectCancellationAndReleaseSeat()
    {
        using var scope = _fixture.CreateScope();

        var admissionService = scope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await admissionService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Expired Cancellation Test {Guid.NewGuid()}",
                TotalSeats = 1
            });

        const string studentId = "44444444444444444";

        var hold = await admissionService.CreateHoldAsync(
            createdClass.Id,
            studentId);

        // Force the hold to expire in the test database.
        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolAdmissionDbContext>();

        int updatedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
         UPDATE holds
         SET expires_at = clock_timestamp() - INTERVAL '1 second'
         WHERE id = {hold.HoldId}
         """);

        Assert.Equal(1, updatedRows);

        // An expired hold cannot be cancelled as an active hold.
        var exception = await Assert.ThrowsAsync<AdmissionException>(
            () => admissionService.CancelHoldAsync(
                hold.HoldId,
                studentId));

        Assert.Equal("HOLD_NOT_ACTIVE", exception.Code);

        var expiredHold = await admissionService.GetHoldByIdAsync(
            hold.HoldId);

        var classStatus = await admissionService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal("EXPIRED", expiredHold.Status);
        Assert.Equal(0, classStatus.HeldSeats);
        Assert.Equal(0, classStatus.ConfirmedSeats);
        Assert.Equal(1, classStatus.AvailableSeats);
    }


    [Fact]
    public async Task CreateHold_WhenMultipleStudentsApplyConcurrently_ShouldNeverOversellSeats()
    {
        // Arrange: create a class with only 3 available seats.
        using var setupScope = _fixture.CreateScope();

        var setupService = setupScope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var createdClass = await setupService.CreateClassAsync(
            new CreateClassRequest
            {
                Name = $"Concurrency Test {Guid.NewGuid()}",
                TotalSeats = 3
            });

        // All requests must start together.
        var startSignal = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<bool> TryCreateHoldAsync(string studentId)
        {
            // Each request gets its own DI scope and DbContext.
            using var scope = _fixture.CreateScope();

            var admissionService = scope.ServiceProvider
                .GetRequiredService<IAdmissionService>();

            await startSignal.Task;

            try
            {
                await admissionService.CreateHoldAsync(
                    createdClass.Id,
                    studentId);

                return true;
            }
            catch (AdmissionException exception)
                when (exception.Code == "SEATS_UNAVAILABLE")
            {
                return false;
            }
        }

        // Arrange 10 requests with 10 unique student IDs.
        var tasks = Enumerable.Range(1, 10)
            .Select(number => TryCreateHoldAsync(number.ToString("D17")))
            .ToArray();

        // Act: release all 10 requests at once.
        startSignal.SetResult();

        var results = await Task.WhenAll(tasks);

        // Assert: exactly 3 requests should succeed.
        Assert.Equal(3, results.Count(result => result));
        Assert.Equal(7, results.Count(result => !result));

        // Verify the final database state using a fresh scope.
        using var verificationScope = _fixture.CreateScope();

        var verificationService = verificationScope.ServiceProvider
            .GetRequiredService<IAdmissionService>();

        var classStatus = await verificationService.GetClassByIdAsync(
            createdClass.Id);

        Assert.Equal(3, classStatus.TotalSeats);
        Assert.Equal(3, classStatus.HeldSeats);
        Assert.Equal(0, classStatus.ConfirmedSeats);
        Assert.Equal(0, classStatus.AvailableSeats);
    }




}
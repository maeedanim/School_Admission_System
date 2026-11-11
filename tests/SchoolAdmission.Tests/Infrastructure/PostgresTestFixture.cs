
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models.Auth;
using SchoolAdmission.BLL.Services;

using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Entities.Identity;
using SchoolAdmission.DAL.Repositories;

namespace SchoolAdmission.Tests.Infrastructure;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                "TEST_DATABASE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "TEST_DATABASE_CONNECTION_STRING environment variable is not configured.");

        var configuration = new ConfigurationManager
        {
            ["HOLD_DURATION_SECONDS"] = "600",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        };

        var services = new ServiceCollection();

        // Register logging and authentication infrastructure.
        services.AddLogging();
        services.AddAuthentication();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<SchoolAdmissionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(10);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SchoolAdmissionDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IAdmissionRepository, AdmissionRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IAccessTokenGenerator, TestAccessTokenGenerator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdmissionService, AdmissionService>();

        _serviceProvider = services.BuildServiceProvider();

        await using var scope = _serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolAdmissionDbContext>();

        await dbContext.Database.MigrateAsync();

        await SeedRolesAsync(scope.ServiceProvider);
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public async Task<(Guid UserId, string BirthRegistrationNumber)>
        CreateStudentAsync()
    {
        using var scope = CreateScope();

        var identityRepository = scope.ServiceProvider
            .GetRequiredService<IIdentityRepository>();

        string uniqueId = Guid.NewGuid().ToString("N");

        string email = $"student-{uniqueId}@integration.test";

        string birthRegistrationNumber =
            GenerateBirthRegistrationNumber();

        var result = await identityRepository.CreateStudentAsync(
            fullName: $"Integration Test Student {uniqueId}",
            email: email,
            password: "TestPassword123",
            birthRegistrationNumber: birthRegistrationNumber);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create integration test student: " +
                string.Join("; ", result.Errors));
        }

        Guid userId = result.UserId
            ?? throw new InvalidOperationException(
                "Student creation succeeded but no user ID was returned.");

        return (userId, birthRegistrationNumber);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        string[] requiredRoles = ["Student", "Admin"];

        foreach (string roleName in requiredRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName));

            if (!result.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to create test role '{roleName}': {errors}");
            }
        }
    }

    private static string GenerateBirthRegistrationNumber()
    {
        var digits = new char[17];

        for (int i = 0; i < digits.Length; i++)
        {
            digits[i] = (char)(
                '0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(digits);
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    private sealed class TestAccessTokenGenerator
        : IAccessTokenGenerator
    {
        public Task<(string Token, DateTime ExpiresAtUtc)> GenerateAsync(
            AuthUserData user,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult((
                Token: "integration-test-access-token",
                ExpiresAtUtc: DateTime.UtcNow.AddMinutes(5)));
        }
    }
}

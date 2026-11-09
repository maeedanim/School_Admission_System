using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Services;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Repositories;

namespace SchoolAdmission.Tests.Infrastructure;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "TEST_DATABASE_CONNECTION_STRING environment variable is not configured.");

        var configuration = new ConfigurationManager
        {
            ["HOLD_DURATION_SECONDS"] = "600"
        };

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<SchoolAdmissionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAdmissionRepository, AdmissionRepository>();
        services.AddScoped<IAdmissionService, AdmissionService>();

        _serviceProvider = services.BuildServiceProvider();

        // Apply EF Core migrations to the dedicated test database.
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolAdmissionDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public Task DisposeAsync()
    {
        return _serviceProvider.DisposeAsync().AsTask();
    }
}
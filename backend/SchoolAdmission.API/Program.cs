
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using SchoolAdmission.API.Middleware;
using SchoolAdmission.API.Services.Auth;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Services;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Entities.Identity;
using SchoolAdmission.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Service Configuration
// --------------------------------------------------

// Controllers and validation responses.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(
                context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "VALIDATION_ERROR",
                Detail = "The request contains invalid data.",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["code"] = "VALIDATION_ERROR";

            var result = new BadRequestObjectResult(problem);
            result.ContentTypes.Add("application/problem+json");

            return result;
        };
    });

// Swagger and JWT Bearer authentication.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

// CORS.
var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"]
    ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --------------------------------------------------
// Database Configuration
// --------------------------------------------------

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<SchoolAdmissionDbContext>(options =>
    options.UseNpgsql(connectionString));

// --------------------------------------------------
// ASP.NET Core Identity
// --------------------------------------------------

builder.Services
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

// --------------------------------------------------
// JWT Authentication
// --------------------------------------------------

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "JWT secret is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

byte[] jwtKeyBytes;

try
{
    jwtKeyBytes = Convert.FromBase64String(jwtSecret);
}
catch (FormatException)
{
    throw new InvalidOperationException(
        "JWT secret must be a valid Base64 string.");
}

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT secret must contain at least 32 bytes.");
}

var signingKey = new SymmetricSecurityKey(jwtKeyBytes);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// --------------------------------------------------
// Dependency Injection
// --------------------------------------------------

// Authentication infrastructure.
builder.Services.AddScoped<IAccessTokenGenerator, JwtTokenService>();
builder.Services.AddScoped<AuthCookieService>();

// Repositories.
builder.Services.AddScoped<IAdmissionRepository, AdmissionRepository>();
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Business services.
builder.Services.AddScoped<IAdmissionService, AdmissionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// --------------------------------------------------
// Application Initialization
// --------------------------------------------------

var app = builder.Build();

await InitializeDatabaseAsync(
    app.Services,
    builder.Configuration);

// --------------------------------------------------
// HTTP Request Pipeline
// --------------------------------------------------

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.MapControllers();

app.Run();

// --------------------------------------------------
// Initialization Methods
// --------------------------------------------------

static async Task InitializeDatabaseAsync(
    IServiceProvider services,
    IConfiguration configuration)
{
    await using var scope = services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<SchoolAdmissionDbContext>();

    await dbContext.Database.MigrateAsync();

    await SeedRolesAsync(scope.ServiceProvider);

    await BootstrapAdminAsync(
        scope.ServiceProvider,
        configuration);
}

// --------------------------------------------------
// Role Seeding
// --------------------------------------------------

static async Task SeedRolesAsync(IServiceProvider services)
{
    var roleManager = services
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var requiredRoles = new[] { "Student", "Admin" };

    foreach (var roleName in requiredRoles)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        var result = await roleManager.CreateAsync(
            new IdentityRole<Guid>(roleName));

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Failed to create role '{roleName}': {errors}");
        }
    }
}

// --------------------------------------------------
// Admin Bootstrap
// --------------------------------------------------

static async Task BootstrapAdminAsync(
    IServiceProvider services,
    IConfiguration configuration)
{
    var adminFullName = configuration["AdminBootstrap:FullName"];
    var adminEmail = configuration["AdminBootstrap:Email"];
    var adminPassword = configuration["AdminBootstrap:Password"];

    var bootstrapValues = new[]
    {
        adminFullName,
        adminEmail,
        adminPassword
    };

    var hasBootstrapConfiguration = bootstrapValues.Any(
        value => !string.IsNullOrWhiteSpace(value));

    if (!hasBootstrapConfiguration)
    {
        return;
    }

    if (bootstrapValues.Any(string.IsNullOrWhiteSpace))
    {
        throw new InvalidOperationException(
            "Admin bootstrap configuration is incomplete. " +
            "FullName, Email, and Password must all be provided.");
    }

    adminFullName = adminFullName!.Trim();
    adminEmail = adminEmail!.Trim();

    var identityRepository = services
        .GetRequiredService<IIdentityRepository>();

    var existingUser = await identityRepository.FindByEmailAsync(
        adminEmail);

    if (existingUser is not null)
    {
        if (!existingUser.Roles.Contains("Admin"))
        {
            throw new InvalidOperationException(
                "The configured Admin email belongs to an account " +
                "without the Admin role.");
        }

        return;
    }

    var result = await identityRepository.CreateAdminAsync(
        adminFullName,
        adminEmail,
        adminPassword!);

    if (!result.Succeeded)
    {
        var errors = string.Join("; ", result.Errors);

        throw new InvalidOperationException(
            $"Failed to provision the Admin account: {errors}");
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Services;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Repositories;
using SchoolAdmission.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register controllers
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

// Read the database connection string
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' is not configured.");

// Register PostgreSQL DbContext
builder.Services.AddDbContext<SchoolAdmissionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IAdmissionRepository, AdmissionRepository>();

// Register business services
builder.Services.AddScoped<IAdmissionService, AdmissionService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure HTTP request pipeline
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Services;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Repositories;
using SchoolAdmission.API.Middleware;
using Microsoft.OpenApi;
using SchoolAdmission.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and configure validation responses.
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

// Register Swagger services.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("StudentId", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Student-Id",
        In = ParameterLocation.Header,
        Description = "Enter your 17-digit student birth registration number."
    });

    options.OperationFilter<StudentIdHeaderOperationFilter>();
});

// Configure CORS for the frontend.
var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"]
    ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Read the database connection string.
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' is not configured.");

// Register PostgreSQL DbContext.
builder.Services.AddDbContext<SchoolAdmissionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories.
builder.Services.AddScoped<IAdmissionRepository, AdmissionRepository>();

// Register business services.
builder.Services.AddScoped<IAdmissionService, AdmissionService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger only in Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure HTTP request pipeline.
app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

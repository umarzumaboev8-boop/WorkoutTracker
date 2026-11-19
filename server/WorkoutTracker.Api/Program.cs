using WorkoutTracker.Api.Services;
using WorkoutTracker.Api.Endpoints;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.DTOs;
using WorkoutTracker.Api.Endpoints;
using WorkoutTracker.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=workouts.db"));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PhotoService>();

builder.Services.AddScoped<IValidator<WorkoutCreateRequest>, WorkoutCreateValidator>();
builder.Services.AddScoped<IValidator<WorkoutUpdateRequest>, WorkoutUpdateValidator>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AngularClient");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (
    RegisterRequest request,
    AuthService authService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) ||
        request.Username.Length < 3 ||
        request.Username.Length > 50 ||
        string.IsNullOrWhiteSpace(request.Password) ||
        request.Password.Length < 8 ||
        request.Password.Length > 100)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Username"] = ["Имя пользователя: от 3 до 50 символов"],
            ["Password"] = ["Пароль: от 8 до 100 символов"]
        });
    }

    try
    {
        var user = await authService.RegisterAsync(
            request.Username,
            request.Password);

        return Results.Created(
            $"/api/auth/{user.Id}",
            new { user.Id, user.Username });
    }
    catch (InvalidOperationException)
    {
        return Results.Conflict(new
        {
            message = "Пользователь уже существует"
        });
    }
});

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    AuthService authService) =>
{
    var token = await authService.LoginAsync(
        request.Username,
        request.Password);

    return token is null
        ? Results.Unauthorized()
        : Results.Ok(new { token });
});

app.MapWorkoutEndpoints();

app.Run();

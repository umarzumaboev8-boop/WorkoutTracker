
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.DTOs;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workouts");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapGet("/{id:int}/photo", GetPhotoAsync);

        group.MapPost("/", CreateAsync).RequireAuthorization();
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization();
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization();
        group.MapPost("/{id:int}/photo", UploadPhotoAsync).RequireAuthorization();
    }

    private static async Task<IResult> GetAllAsync(
        ApplicationDbContext db,
        int page = 1,
        int pageSize = 10,
        int? sportTypeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string sortBy = "date",
        string sortOrder = "desc")
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return Results.BadRequest(new { message = "Invalid pagination parameters" });

        if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            return Results.BadRequest(new { message = "Invalid date range" });

        if (sortBy != "date" && sortBy != "duration")
            return Results.BadRequest(new { message = "sortBy must be date or duration" });

        if (sortOrder != "asc" && sortOrder != "desc")
            return Results.BadRequest(new { message = "sortOrder must be asc or desc" });

        var query = db.Workouts
            .AsNoTracking()
            .Include(x => x.SportType)
            .AsQueryable();

        if (sportTypeId.HasValue)
            query = query.Where(x => x.SportTypeId == sportTypeId.Value);

        if (dateFrom.HasValue)
            query = query.Where(x => x.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.Date <= dateTo.Value);

        query = (sortBy, sortOrder) switch
        {
            ("date", "asc") => query.OrderBy(x => x.Date),
            ("date", "desc") => query.OrderByDescending(x => x.Date),
            ("duration", "asc") => query.OrderBy(x => x.DurationMinutes),
            ("duration", "desc") => query.OrderByDescending(x => x.DurationMinutes),
            _ => query
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WorkoutResponse(
                x.Id, x.Title, x.Date, x.DurationMinutes,
                x.Calories, x.Notes, x.SportTypeId,
                x.SportType!.Name, x.PhotoFileName, x.AuthorId))
            .ToListAsync();

        var response = new PagedResponse<WorkoutResponse>(
            items, page, pageSize, total,
            (int)Math.Ceiling(total / (double)pageSize),
            items.Sum(x => x.DurationMinutes),
            page > 1,
            page * pageSize < total);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        int id, ApplicationDbContext db)
    {
        var workout = await db.Workouts
            .AsNoTracking()
            .Include(x => x.SportType)
            .FirstOrDefaultAsync(x => x.Id == id);

        return workout is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(workout));
    }

    private static async Task<IResult> CreateAsync(
        WorkoutCreateRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IValidator<WorkoutCreateRequest> validator)
    {
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
            return ValidationResult(validation);

        var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (authorId is null)
            return Results.Unauthorized();

        if (!await db.SportTypes.AnyAsync(x => x.Id == request.SportTypeId))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["SportTypeId"] = ["Sport type does not exist"]
            });

        var workout = new Workout
        {
            Title = request.Title.Trim(),
            Date = request.Date,
            DurationMinutes = request.DurationMinutes,
            Calories = request.Calories,
            Notes = request.Notes,
            SportTypeId = request.SportTypeId,
            AuthorId = authorId
        };

        db.Workouts.Add(workout);
        await db.SaveChangesAsync();
        await db.Entry(workout).Reference(x => x.SportType).LoadAsync();

        return Results.Created($"/api/workouts/{workout.Id}", ToResponse(workout));
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        WorkoutUpdateRequest request,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IValidator<WorkoutUpdateRequest> validator)
    {
        var workout = await db.Workouts
            .Include(x => x.SportType)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (workout is null)
            return Results.NotFound();

        var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (workout.AuthorId != authorId)
            return Results.StatusCode(403);

        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
            return ValidationResult(validation);

        if (!await db.SportTypes.AnyAsync(x => x.Id == request.SportTypeId))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["SportTypeId"] = ["Sport type does not exist"]
            });

        workout.Title = request.Title.Trim();
        workout.Date = request.Date;
        workout.DurationMinutes = request.DurationMinutes;
        workout.Calories = request.Calories;
        workout.Notes = request.Notes;
        workout.SportTypeId = request.SportTypeId;

        await db.SaveChangesAsync();
        await db.Entry(workout).Reference(x => x.SportType).LoadAsync();

        return Results.Ok(ToResponse(workout));
    }

   
private static async Task<IResult> DeleteAsync(
    int id,
    ClaimsPrincipal user,
    ApplicationDbContext db,
    IWebHostEnvironment environment)
{
    var workout = await db.Workouts.FindAsync(id);

    if (workout is null)
        return Results.NotFound();

    var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (workout.AuthorId != authorId)
        return Results.StatusCode(403);

    if (!string.IsNullOrEmpty(workout.PhotoFileName))
    {
        var uploads = Path.Combine(
            environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
            "uploads");

        var photoPath = Path.Combine(uploads, workout.PhotoFileName);

        if (File.Exists(photoPath))
            File.Delete(photoPath);
    }

    db.Workouts.Remove(workout);
    await db.SaveChangesAsync();

    return Results.NoContent();
}

    private static string appEnvironmentPath(ApplicationDbContext db)
    {
        return Directory.GetCurrentDirectory();
    }

    private static async Task<IResult> UploadPhotoAsync(
        int id,
        IFormFile file,
        ClaimsPrincipal user,
        ApplicationDbContext db,
        IWebHostEnvironment environment)
    {
        var workout = await db.Workouts.FindAsync(id);

        if (workout is null)
            return Results.NotFound();

        var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (workout.AuthorId != authorId)
            return Results.StatusCode(403);

        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            return Results.BadRequest(new { message = "File must be between 1 byte and 5 MB" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowed.Contains(extension))
            return Results.BadRequest(new { message = "Only JPG, PNG and WEBP are allowed" });

        var uploads = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploads, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        if (!string.IsNullOrEmpty(workout.PhotoFileName))
        {
            var oldPath = Path.Combine(uploads, workout.PhotoFileName);
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        workout.PhotoFileName = fileName;
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            workout.Id,
            workout.PhotoFileName,
            PhotoUrl = $"/api/workouts/{id}/photo"
        });
    }

    private static async Task<IResult> GetPhotoAsync(
        int id, ApplicationDbContext db, IWebHostEnvironment environment)
    {
        var workout = await db.Workouts.FindAsync(id);

        if (workout is null || string.IsNullOrEmpty(workout.PhotoFileName))
            return Results.NotFound();

        var uploads = Path.Combine(
            environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
            "uploads");

        var path = Path.Combine(uploads, workout.PhotoFileName);

        if (!File.Exists(path))
            return Results.NotFound();

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return Results.File(path, contentType);
    }

    private static WorkoutResponse ToResponse(Workout workout)
    {
        return new WorkoutResponse(
            workout.Id, workout.Title, workout.Date,
            workout.DurationMinutes, workout.Calories,
            workout.Notes, workout.SportTypeId,
            workout.SportType?.Name ?? string.Empty,
            workout.PhotoFileName, workout.AuthorId);
    }

    private static IResult ValidationResult(
        FluentValidation.Results.ValidationResult validation)
    {
        var errors = validation.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }
}

using Microsoft.AspNetCore.Http;

namespace WorkoutTracker.Api.Services;

public class PhotoService
{
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp"
    ];

    public PhotoService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile photo)
    {
        if (photo.Length == 0 || photo.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Размер фотографии должен быть от 1 байта до 5 МБ.");

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Допустимые форматы: JPG, PNG, WEBP.");

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await photo.CopyToAsync(stream);

        return fileName;
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var safeName = Path.GetFileName(fileName);

        var path = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads",
            safeName);

        if (File.Exists(path))
            File.Delete(path);
    }
}

namespace WorkoutTracker.Api.DTOs;

public record WorkoutResponse(
    int Id,
    string Title,
    DateTime Date,
    int DurationMinutes,
    int Calories,
    string? Notes,
    int SportTypeId,
    string SportTypeName,
    string? PhotoUrl,
    string AuthorId
);

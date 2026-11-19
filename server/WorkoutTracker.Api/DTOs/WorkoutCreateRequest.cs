
namespace WorkoutTracker.Api.DTOs;

public record WorkoutCreateRequest(
    string Title,
    DateTime Date,
    int DurationMinutes,
    int Calories,
    string? Notes,
    int SportTypeId
);


namespace WorkoutTracker.Api.DTOs;

public record WorkoutUpdateRequest(
    string Title,
    DateTime Date,
    int DurationMinutes,
    int Calories,
    string? Notes,
    int SportTypeId
);

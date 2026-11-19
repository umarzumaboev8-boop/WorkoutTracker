
namespace WorkoutTracker.Api.DTOs;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPages,
    int TotalMinutes,
    bool HasPrevious,
    bool HasNext
);

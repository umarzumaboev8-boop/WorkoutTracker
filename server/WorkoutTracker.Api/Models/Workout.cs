
namespace WorkoutTracker.Api.Models;

public class Workout
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int DurationMinutes { get; set; }

    public int Calories { get; set; }

    public string? Notes { get; set; }

    public int SportTypeId { get; set; }

    public SportType? SportType { get; set; }

    public string? PhotoFileName { get; set; }

    public string AuthorId { get; set; } = string.Empty;
}

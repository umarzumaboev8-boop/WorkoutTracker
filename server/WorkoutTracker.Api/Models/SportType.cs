
namespace WorkoutTracker.Api.Models;

public class SportType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}

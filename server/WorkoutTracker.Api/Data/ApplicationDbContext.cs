
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workout> Workouts => Set<Workout>();

    public DbSet<SportType> SportTypes => Set<SportType>();

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Username)
            .IsUnique();

        modelBuilder.Entity<SportType>().HasData(
            new SportType { Id = 1, Name = "Boxing" },
            new SportType { Id = 2, Name = "Running" },
            new SportType { Id = 3, Name = "Swimming" },
            new SportType { Id = 4, Name = "Gym" },
            new SportType { Id = 5, Name = "Cycling" }
        );

        modelBuilder.Entity<Workout>().HasData(
            new Workout { Id = 1, Title = "Morning Boxing", Date = new DateTime(2026, 1, 10), DurationMinutes = 60, Calories = 450, SportTypeId = 1, AuthorId = "demo-user" },
            new Workout { Id = 2, Title = "Morning Run", Date = new DateTime(2026, 1, 11), DurationMinutes = 40, Calories = 300, SportTypeId = 2, AuthorId = "demo-user" },
            new Workout { Id = 3, Title = "Swimming Session", Date = new DateTime(2026, 1, 12), DurationMinutes = 50, Calories = 400, SportTypeId = 3, AuthorId = "demo-user" },
            new Workout { Id = 4, Title = "Gym Training", Date = new DateTime(2026, 1, 13), DurationMinutes = 75, Calories = 500, SportTypeId = 4, AuthorId = "demo-user" },
            new Workout { Id = 5, Title = "Cycling Session", Date = new DateTime(2026, 1, 14), DurationMinutes = 90, Calories = 600, SportTypeId = 5, AuthorId = "demo-user" }
        );
    }
}

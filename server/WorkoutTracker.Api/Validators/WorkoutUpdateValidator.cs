
using FluentValidation;
using WorkoutTracker.Api.DTOs;

namespace WorkoutTracker.Api.Validators;

public class WorkoutUpdateValidator : AbstractValidator<WorkoutUpdateRequest>
{
    public WorkoutUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(100).WithMessage("Максимум 100 символов");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Дата обязательна")
            .Must(date => date <= DateTime.Now)
            .WithMessage("Дата тренировки не может быть в будущем");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("Длительность должна быть от 1 до 1440 минут");

        RuleFor(x => x.Calories)
            .InclusiveBetween(0, 10000)
            .WithMessage("Калории должны быть от 0 до 10000");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Заметки не должны превышать 1000 символов");

        RuleFor(x => x.SportTypeId)
            .GreaterThan(0)
            .WithMessage("Выберите вид спорта");
    }
}

using FitCoach.API.DTOs.Exercise;

namespace FitCoach.API.DTOs.Workout;

public record WorkoutDayDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    string Label,
    string? Notes,
    int OrderIndex,
    List<PlanExerciseDto> Exercises
);

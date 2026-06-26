using FitCoach.API.DTOs.Exercise;

namespace FitCoach.API.DTOs.Workout;

public record CreateWorkoutDayRequest(
    DayOfWeek DayOfWeek,
    string Label,
    string? Notes,
    int OrderIndex,
    List<CreatePlanExerciseRequest> Exercises
);

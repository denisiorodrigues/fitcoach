namespace FitCoach.API.DTOs.Auth;

public record SessionSetDto(
    Guid Id,
    Guid PlanExerciseId,
    string ExerciseName,
    int SetNumber,
    int RepsDone,
    float WeightKg,
    DateTime LoggedAt
);

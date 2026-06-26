namespace FitCoach.API.DTOs.Auth;

public record LogSetRequest(
    Guid PlanExerciseId,
    int SetNumber,
    int RepsDone,
    float WeightKg
);

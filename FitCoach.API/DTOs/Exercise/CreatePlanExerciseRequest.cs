namespace FitCoach.API.DTOs.Exercise;

public record CreatePlanExerciseRequest(
    Guid ExerciseId,
    int Sets,
    string Reps,
    float? WeightKg,
    int RestSeconds,
    int OrderIndex,
    string? CoachNotes
);

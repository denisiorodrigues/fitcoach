namespace FitCoach.API.DTOs.Exercise;

public record PlanExerciseDto(
    Guid Id,
    ExerciseDto Exercise,
    int Sets,
    string Reps,
    float? WeightKg,
    int RestSeconds,
    int OrderIndex,
    string? CoachNotes
);

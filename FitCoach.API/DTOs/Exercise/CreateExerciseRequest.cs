namespace FitCoach.API.DTOs.Exercise;

public record CreateExerciseRequest(
    string Name,
    string MuscleGroup,
    string Equipment,
    string? Instructions,
    string? VideoUrl,
    string? ThumbnailUrl
);

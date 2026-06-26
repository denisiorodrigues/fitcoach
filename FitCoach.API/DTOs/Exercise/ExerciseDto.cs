namespace FitCoach.API.DTOs.Exercise;

public record ExerciseDto(
    Guid Id,
    string Name,
    string MuscleGroup,
    string Equipment,
    string? Instructions,
    string? VideoUrl,
    string? ThumbnailUrl,
    bool IsGlobal
);

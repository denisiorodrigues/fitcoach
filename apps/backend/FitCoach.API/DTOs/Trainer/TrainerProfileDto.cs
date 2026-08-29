using FitCoach.API.DTOs.User;

namespace FitCoach.API.DTOs.Trainer;

public record TrainerProfileDto(
    Guid Id,
    UserDto User,
    string? Bio,
    string? Specialty,
    string? CrefNumber,
    int TotalStudents,
    int TotalPlans
);

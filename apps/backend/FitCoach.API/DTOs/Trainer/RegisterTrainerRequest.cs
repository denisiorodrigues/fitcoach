namespace FitCoach.API.DTOs.Trainer;

public record RegisterTrainerRequest(
    string Name,
    string Email,
    string Password,
    string? Specialty,
    string? CrefNumber
);

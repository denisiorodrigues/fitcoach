namespace FitCoach.API.DTOs.User;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? AvatarUrl
);

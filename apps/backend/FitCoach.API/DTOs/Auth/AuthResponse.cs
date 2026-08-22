using FitCoach.API.DTOs.User;

namespace FitCoach.API.DTOs.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User
);

namespace FitCoach.API.DTOs.Student;

public record RegisterStudentRequest(
    string Name,
    string Email,
    string Password,
    string TrainerInviteCode   // trainer shares this code to onboard students
);

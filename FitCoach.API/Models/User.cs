using System.ComponentModel.DataAnnotations;

namespace FitCoach.API.Models;

public class User
{
    [Key] 
    public Guid Id { get; set; }

    [Required, MaxLength(120)] 
    public string Name { get; set; }

    [Required, MaxLength(200)] 
    public string Email { get; set; }
    
    [Required] 
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public TrainerProfile? TrainerProfile { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}

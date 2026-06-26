using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class TrainerProfile
{
    [Key] 
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [MaxLength(500)] 
    public string? Bio { get; set; }

    [MaxLength(200)] 
    public string? Specialty { get; set; }
    
    public string? CrefNumber { get; set; }

    [ForeignKey("UserId")] 
    public User User { get; set; }
    
    public ICollection<StudentProfile> Students { get; set; }
    
    public ICollection<Exercise> Exercises { get; set; }
    
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; }
}

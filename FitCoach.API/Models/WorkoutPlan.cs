using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class WorkoutPlan
{
    [Key] 
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    
    [Required, MaxLength(150)] 
    public string Name { get; set; }
    
    [MaxLength(500)] 
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    [ForeignKey("TrainerId")] 
    public TrainerProfile Trainer { get; set; }

    [ForeignKey("StudentId")] 
    public StudentProfile Student { get; set; }
    
    public ICollection<WorkoutDay> Days { get; set; }
}

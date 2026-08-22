using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class Exercise
{
    [Key] 
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    
    [Required, MaxLength(150)] 
    public string Name { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
    
    [MaxLength(1000)] 
    public string? Instructions { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsGlobal { get; set; }
    public DateTime CreatedAt { get; set; }

    [ForeignKey("TrainerId")] 
    public TrainerProfile Trainer { get; set; }
    public ICollection<PlanExercise> PlanExercises { get; set; }
}

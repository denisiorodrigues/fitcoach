using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class PlanExercise
{
    [Key] 
    public Guid Id { get; set; }
    public Guid WorkoutDayId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    
    [MaxLength(20)] 
    public string Reps { get; set; }
    public float? WeightKg { get; set; }
    public int RestSeconds { get; set; }
    public int OrderIndex { get; set; }

    [MaxLength(300)] 
    public string? CoachNotes { get; set; }

    [ForeignKey("WorkoutDayId")] 
    public WorkoutDay WorkoutDay { get; set; }

    [ForeignKey("ExerciseId")] 
    public Exercise Exercise { get; set; }
    public ICollection<SessionSet> SessionSets { get; set; }
}

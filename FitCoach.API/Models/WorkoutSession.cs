using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class WorkoutSession
{
    [Key] 
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid WorkoutDayId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? AvgHeartRate { get; set; }
    public int? CaloriesBurned { get; set; }
    public int? DurationSeconds { get; set; }
    [MaxLength(500)] 
    public string? StudentNotes { get; set; }

    [ForeignKey("StudentId")] 
    public StudentProfile Student { get; set; }
    [ForeignKey("WorkoutDayId")] 
    public WorkoutDay WorkoutDay { get; set; }
    public ICollection<SessionSet> Sets { get; set; }
}

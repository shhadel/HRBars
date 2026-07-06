using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.InterviewScore;

public class SaveInterviewScoresRequest
{
    [Required]
    public List<InterviewCompetencyScoreRequest> Scores { get; set; } = new();
}
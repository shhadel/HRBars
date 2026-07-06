using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.InterviewScore;

public class InterviewCompetencyScoreRequest
{
    [Required]
    public Guid CompetencyId { get; set; }

    [Range(0, 10)]
    public short Score { get; set; }

    [Range(1, 10)]
    public short Weight { get; set; } = 1;

    [Range(1, 100)]
    public short MaxScore { get; set; } = 10;

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
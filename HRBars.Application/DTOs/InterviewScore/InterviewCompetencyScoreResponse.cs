namespace HRBars.Application.DTOs.InterviewScore;

public class InterviewCompetencyScoreResponse
{
    public Guid Id { get; set; }

    public Guid CompetencyId { get; set; }

    public string CompetencyName { get; set; } = string.Empty;

    public short Score { get; set; }

    public short Weight { get; set; }

    public short MaxScore { get; set; }

    public string? Comment { get; set; }
}
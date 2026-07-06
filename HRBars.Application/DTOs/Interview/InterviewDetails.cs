namespace HRBars.Application.DTOs.Interview;

public class InterviewDetails : InterviewResponse
{
    public short? DurationMinutes { get; set; }

    public string? Location { get; set; }

    public string? Plan { get; set; }

    public string? DecisionComment { get; set; }

    public DateTime? DecisionDate { get; set; }

    public string? DecidedBy { get; set; }
}
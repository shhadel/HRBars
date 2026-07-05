namespace HRBars.Application.DTOs.Application;

public class ApplicationInterviewDto
{
    public Guid Id { get; set; }
    public DateTime InterviewDate { get; set; }
    public string Format { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? Plan { get; set; }
    public string? Result { get; set; }
    public string? DecisionComment { get; set; }
    public DateTime? DecisionDate { get; set; }
}
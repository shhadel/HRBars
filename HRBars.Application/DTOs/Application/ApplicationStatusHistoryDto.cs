namespace HRBars.Application.DTOs.Application;

public class ApplicationStatusHistoryDto
{
    public Guid Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime ChangedAt { get; set; }
}
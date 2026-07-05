namespace HRBars.Application.DTOs.Application;

public class ApplicationVacancyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
}
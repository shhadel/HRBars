namespace HRBars.Application.DTOs.Vacancy;

public class VacancyListResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ApplicationsCount { get; set; }
}
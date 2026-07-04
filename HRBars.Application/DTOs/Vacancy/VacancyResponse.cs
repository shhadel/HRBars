namespace HRBars.Application.DTOs.Vacancy
{
    public class VacancyResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ApplicationsCount { get; set; }
        public List<string> Competencies { get; set; } = new();
    }
}

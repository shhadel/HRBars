namespace HRBars.Application.DTOs.Vacancy
{
    public class CompetencyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
    }
}

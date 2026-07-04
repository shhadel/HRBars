namespace HRBars.Application.DTOs.Vacancy
{
    public class VacancyDetails : VacancyResponse
    {
        public List<ApplicationBrief> RecentApplications { get; set; } = new();
        public List<CompetencyDetails> CompetencyDetails { get; set; } = new();
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}

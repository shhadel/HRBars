namespace HRBars.Application.DTOs.Vacancy
{
    public class ApplicationBrief
    {
        public Guid Id { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }
}

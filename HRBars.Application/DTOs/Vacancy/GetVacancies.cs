namespace HRBars.Application.DTOs.Vacancy
{
    public class GetVacancies
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Department { get; set; }
        public bool? IsArchived { get; set; }
        public bool IncludeArchived { get; set; } = false;
    }
}

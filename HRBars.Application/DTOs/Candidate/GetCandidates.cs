namespace HRBars.Application.DTOs.Candidate
{
    public class GetCandidates
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? Search { get; set; }

        public string? City { get; set; }
    }
}
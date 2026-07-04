namespace HRBars.Application.DTOs.Vacancy
{
    public class CompetencyDetails : CompetencyResponse
    {
        public int Weight { get; set; } = 1;
        public int MaxScore { get; set; } = 10;
    }
}

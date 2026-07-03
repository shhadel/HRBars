namespace HRBars.Application.DTOs.Candidate
{
    public class CandidateDetails : CandidateResponse
    {
        public string? ResumeStorageKey { get; set; }

        public List<EducationResponse> Educations { get; set; } = new();

        public List<WorkExperienceResponse> WorkExperiences { get; set; } = new();

        public int ApplicationsCount { get; set; }
    }
}
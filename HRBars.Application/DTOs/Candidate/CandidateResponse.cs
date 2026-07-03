namespace HRBars.Application.DTOs.Candidate
{
    public class CandidateResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? City { get; set; }

        public string? Skills { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
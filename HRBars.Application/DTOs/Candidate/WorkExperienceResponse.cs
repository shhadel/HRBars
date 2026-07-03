namespace HRBars.Application.DTOs.Candidate
{
	public class WorkExperienceResponse
	{
		public Guid Id { get; set; }

		public string Company { get; set; } = string.Empty;

		public string Position { get; set; } = string.Empty;

		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public string? Description { get; set; }
	}
}
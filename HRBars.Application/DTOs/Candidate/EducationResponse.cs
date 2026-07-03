namespace HRBars.Application.DTOs.Candidate
{
	public class EducationResponse
	{
		public Guid Id { get; set; }

		public string Institution { get; set; } = string.Empty;

		public string? Faculty { get; set; }

		public string? Degree { get; set; }

		public short? StartYear { get; set; }

		public short? EndYear { get; set; }
	}
}
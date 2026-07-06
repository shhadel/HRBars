using System.ComponentModel.DataAnnotations;
using HRBars.Domain.Enums;

namespace HRBars.Application.DTOs.Interview;

public class CreateInterview
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    public DateTime InterviewDate { get; set; }

    [Required]
    public InterviewFormat Format { get; set; }

    [Range(1, 480)]
    public short? DurationMinutes { get; set; }

    [MaxLength(255)]
    public string? Location { get; set; }

    [MaxLength(5000)]
    public string? Plan { get; set; }
}
using System.ComponentModel.DataAnnotations;
using HRBars.Domain.Enums;

namespace HRBars.Application.DTOs.Interview;

public class MakeDecision
{
    [Required]
    public InterviewResult Result { get; set; }

    [MaxLength(2000)]
    public string? DecisionComment { get; set; }
}
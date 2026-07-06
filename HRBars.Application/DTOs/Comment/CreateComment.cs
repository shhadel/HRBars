using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Comment;

public class CreateComment
{
    [Required]
    public Guid InterviewId { get; set; }

    [MaxLength(100)]
    public string? Section { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Text { get; set; } = string.Empty;
}
namespace HRBars.Application.DTOs.Comment;

public class CommentResponse
{
    public Guid Id { get; set; }

    public string? Section { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
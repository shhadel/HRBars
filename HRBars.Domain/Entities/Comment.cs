namespace HRBars.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public string? Section { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public Guid InterviewId { get; set; }
    public Interview Interview { get; set; } = null!;
    
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
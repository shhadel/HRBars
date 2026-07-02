namespace HRBars.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ChangedAt { get; set; }
    
    public Guid ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;
}
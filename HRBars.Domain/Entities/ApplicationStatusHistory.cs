namespace HRBars.Domain.Entities;

public class ApplicationStatusHistory
{
    public Guid Id { get; set; }
    public short Status { get; set; }
    public string? Comment { get; set; }
    public DateTime ChangedAt { get; set; }
    
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
}
namespace HRBars.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
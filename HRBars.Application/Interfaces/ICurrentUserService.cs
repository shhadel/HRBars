using HRBars.Domain.Entities;

namespace HRBars.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    Task<User?> GetUserAsync();
    bool HasPermission(string permissionName);
    Task<List<string>> GetPermissionsAsync();
}
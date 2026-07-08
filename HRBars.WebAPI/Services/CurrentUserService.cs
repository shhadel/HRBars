using System.Security.Claims;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HRBars.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string PERMISSIONS_CACHE_KEY = "user_permissions_{0}";

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _cache = cache;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public async Task<User?> GetUserAsync()
    {
        if (!UserId.HasValue)
            return null;

        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == UserId.Value);
    }

    public bool HasPermission(string permissionName)
    {
        if (!IsAuthenticated || !UserId.HasValue)
            return false;

        // Проверяем кэш
        var cacheKey = string.Format(PERMISSIONS_CACHE_KEY, UserId.Value);
        if (_cache.TryGetValue(cacheKey, out HashSet<string> cachedPermissions))
        {
            return cachedPermissions.Contains(permissionName);
        }

        // Загружаем права из БД
        var permissions = _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == UserId.Value)
            .Select(up => up.Permission.Name)
            .ToHashSet();

        // Сохраняем в кэш на 5 минут
        _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions.Contains(permissionName);
    }

    public async Task<List<string>> GetPermissionsAsync()
    {
        if (!UserId.HasValue)
            return new List<string>();

        return await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == UserId.Value)
            .Select(up => up.Permission.Name)
            .ToListAsync();
    }
}
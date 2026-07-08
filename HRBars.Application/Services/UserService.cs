using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace HRBars.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context,
            ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResult<UserResponse>> GetUsersAsync(GetUsers query)
        {
            var usersQuery = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Фильтрация
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    (u.MiddleName != null && u.MiddleName.ToLower().Contains(search)) ||
                    u.Email.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                usersQuery = usersQuery.Where(u => u.Role.Name == query.Role);
            }

            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
            }

            var totalCount = await usersQuery.CountAsync();

            var items = await usersQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    FullName = $"{u.LastName} {u.FirstName} {u.MiddleName}".Trim(),
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Permissions = _context.UserPermissions
                        .Where(up => up.UserId == u.Id)
                        .Select(up => up.Permission.Name)
                        .ToList()
                })
                .ToListAsync();

            return new PaginatedResult<UserResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<UserDetails?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            var permissions = await _context.UserPermissions
                .Where(up => up.UserId == id)
                .Select(up => up.Permission.Name)
                .ToListAsync();

            return new UserDetails
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim(),
                Email = user.Email,
                RoleName = user.Role.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Permissions = permissions,
                CreatedApplicationsCount = await _context.Applications.CountAsync(a => a.CreatedByUserId == id),
                CreatedInterviewsCount = await _context.Interviews.CountAsync(i => i.CreatedByUserId == id),
                DecidedInterviewsCount = await _context.Interviews.CountAsync(i => i.DecidedByUserId == id),
                CommentsCount = await _context.Comments.CountAsync(c => c.CreatedByUserId == id)
            };
        }

        public async Task<UserResponse> CreateUserAsync(CreateUser request, Guid createdByUserId)
        {
            // Проверка на существующий Email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new InvalidOperationException("Пользователь с таким Email уже существует");

            // Поиск роли
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.RoleName);

            if (role == null)
                throw new InvalidOperationException($"Роль '{request.RoleName}' не найдена");

            // Создание пользователя
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);

            // Добавление прав, если указаны
            if (request.Permissions != null && request.Permissions.Any())
            {
                var permissions = await _context.Permissions
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    _context.UserPermissions.Add(new UserPermission
                    {
                        UserId = user.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow,
                        GrantedBy = createdByUserId
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Получение созданного пользователя с ролью
            var createdUser = await GetUserByIdAsync(user.Id);

            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim(),
                Email = user.Email,
                RoleName = role.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Permissions = request.Permissions ?? new List<string>()
            };
        }

        public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUser request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            // Обновление полей
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (request.MiddleName != null)
                user.MiddleName = request.MiddleName;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Проверка на существующий Email (кроме текущего пользователя)
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id);

                if (existingUser != null)
                    throw new InvalidOperationException("Этот Email уже используется другим пользователем");

                user.Email = request.Email;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = HashPassword(request.Password);

            if (!string.IsNullOrWhiteSpace(request.RoleName))
            {
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == request.RoleName);

                if (role == null)
                    throw new InvalidOperationException($"Роль '{request.RoleName}' не найдена");

                user.RoleId = role.Id;
            }

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            // Обновление прав
            if (request.Permissions != null)
            {
                // Удаляем старые права
                var oldPermissions = _context.UserPermissions
                    .Where(up => up.UserId == id);
                _context.UserPermissions.RemoveRange(oldPermissions);

                // Добавляем новые
                var permissions = await _context.Permissions
                    .Where(p => request.Permissions.Contains(p.Name))
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    _context.UserPermissions.Add(new UserPermission
                    {
                        UserId = user.Id,
                        PermissionId = permission.Id,
                        GrantedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            var updatedUser = await GetUserByIdAsync(id);

            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim(),
                Email = user.Email,
                RoleName = user.Role.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Permissions = request.Permissions ?? new List<string>()
            };
        }

        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return false;

            // Нельзя деактивировать самого себя
            // (проверка будет на уровне контроллера)

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return false;

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim(),
                Email = user.Email,
                RoleName = user.Role.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
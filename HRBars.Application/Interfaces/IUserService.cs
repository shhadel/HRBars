using HRBars.Application.DTOs.User;

namespace HRBars.Application.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResult<UserResponse>> GetUsersAsync(GetUsers query);
        Task<UserDetails?> GetUserByIdAsync(Guid id);
        Task<UserResponse> CreateUserAsync(CreateUser request, Guid createdByUserId);
        Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUser request);
        Task<bool> DeactivateUserAsync(Guid id);
        Task<bool> ActivateUserAsync(Guid id);
        Task<UserResponse?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(Guid id);
    }
}
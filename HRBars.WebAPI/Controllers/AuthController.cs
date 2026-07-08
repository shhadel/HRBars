using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRBars.Application.Services;
using HRBars.Application.DTOs;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(TokenService tokenService, AppDbContext dbContext, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .Include(q => q.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Login);

        if (user == null || !HRBars.Application.Hasher.VerifyPasswordHash(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        if (!user.IsActive)
            return Unauthorized(new { message = "User account is disabled" });

        var accessToken = _tokenService.GenerateAccessToken(user);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var refreshToken = await _tokenService.CreateRefreshTokenAsync(
            user,
            ipAddress,
            userAgent);

        var permissionNames = user.UserPermissions
            .Select(up => up.Permission.Name)
            .ToList();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryMinutes"]) * 60,
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name,
                Permissions = permissionNames
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _tokenService.RefreshAccessTokenAsync(request.RefreshToken, ipAddress);

        if (result == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        return Ok(new RefreshResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryMinutes"]) * 60
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        if (request?.RefreshToken != null)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);
        }
        else
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw new UnauthorizedAccessException());
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _tokenService.RevokeAllUserTokensAsync(userId, ipAddress);
        }

        return Ok(new { message = "Logged out successfully" });
    }
}
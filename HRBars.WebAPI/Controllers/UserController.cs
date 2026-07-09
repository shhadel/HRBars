using HRBars.Application.DTOs;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using HRBars.Application.Services;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех пользователей
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetUsers
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Role = role,
            IsActive = isActive
        };

        var result = await _userService.GetUsersAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Получить карточку пользователя по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Пользователь не авторизован" });

            var createdUser = await _userService.CreateUserAsync(request, Guid.Parse(userIdClaim));
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Email уже существует"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пользователя");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать пользователя
    /// </summary>
    /// <remarks>Доступно только администратору</remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUser request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updatedUser = await _userService.UpdateUserAsync(id, request);

            if (updatedUser == null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Email уже используется"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {UserId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Деактивировать пользователя (мягкое удаление)
    /// </summary>
    /// <remarks>
    /// Пользователь не удаляется физически, а только деактивируется.
    /// Доступно только администратору.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var result = await _userService.DeactivateUserAsync(id);

            if (!result)
                return NotFound(new { message = "Пользователь не найден" });

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("нельзя деактивировать"))
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при деактивации пользователя {UserId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить информацию о текущем авторизованном пользователе
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(guid);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    /// <summary>
    /// Деактивировать пользователя (мягкое удаление)
    /// </summary>
    /// <remarks>
    /// Пользователь не удаляется физически, а только деактивируется.
    /// Доступно только администратору.
    /// </remarks>
    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var result = await _userService.ActivateUserAsync(id);

            if (!result)
                return NotFound(new { message = "Пользователь не найден" });

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("нельзя деактивировать"))
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при деактивации пользователя {UserId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("permissions")]
    [ProducesResponseType(typeof(UserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAvailablePermissions(Guid id)
    {
        var permissions = await _userService.GetAvailablePermissionsAsync();

        if (permissions == null)
            return NotFound(new { message = "Прав нет" });

        return Ok(permissions);
    }
}
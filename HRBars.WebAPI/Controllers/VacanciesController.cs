using Microsoft.AspNetCore.Mvc;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using HRBars.Application.DTOs.Vacancy;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/vacancies")]
public class VacanciesController : ControllerBase
{
    private readonly IVacancyService _vacancyService;
    //private readonly IAuditService _auditService;
    private readonly ILogger<VacanciesController> _logger;

    public VacanciesController(
        IVacancyService vacancyService,
        //IAuditService auditService,
        ILogger<VacanciesController> logger)
    {
        _vacancyService = vacancyService;
        //_auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех вакансий
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<VacancyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacancies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] bool? isArchived = false,
        [FromQuery] bool includeArchived = false)
    {
        var query = new GetVacancies
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Department = department,
            IsArchived = includeArchived ? null : false, // По умолчанию не показываем архивные
            IncludeArchived = includeArchived
        };

        var result = await _vacancyService.GetVacanciesAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Получить карточку вакансии по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VacancyDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacancyById(Guid id)
    {
        var vacancy = await _vacancyService.GetVacancyByIdAsync(id);

        if (vacancy == null)
            return NotFound(new { message = "Вакансия не найдена" });

        return Ok(vacancy);
    }

    /// <summary>
    /// Создать новую вакансию
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVacancy([FromBody] CreateVacancy request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var createdVacancy = await _vacancyService.CreateVacancyAsync(request, userId);

            // Логируем создание
            /*await _auditService.LogAsync(
                entityName: "Vacancy",
                entityId: createdVacancy.Id,
                action: "Create",
                description: $"Создана вакансия '{createdVacancy.Title}'",
                userId: userId
            );*/

            return CreatedAtAction(nameof(GetVacancyById), new { id = createdVacancy.Id }, createdVacancy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании вакансии");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать вакансию
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateVacancy(Guid id, [FromBody] UpdateVacancy request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var updatedVacancy = await _vacancyService.UpdateVacancyAsync(id, request, userId);

            if (updatedVacancy == null)
                return NotFound(new { message = "Вакансия не найдена" });

            // Логируем обновление
            /*await _auditService.LogAsync(
                entityName: "Vacancy",
                entityId: id,
                action: "Update",
                description: $"Обновлена вакансия '{updatedVacancy.Title}'",
                userId: userId
            );*/

            return Ok(updatedVacancy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Архивировать вакансию
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveVacancy(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var vacancy = await _vacancyService.GetVacancyByIdAsync(id);

            if (vacancy == null)
                return NotFound(new { message = "Вакансия не найдена" });

            if (vacancy.IsArchived)
                return BadRequest(new { message = "Вакансия уже находится в архиве" });

            await _vacancyService.ArchiveVacancyAsync(id, userId);

            // Логируем архивацию
            /*await _auditService.LogAsync(
                entityName: "Vacancy",
                entityId: id,
                action: "Archive",
                description: $"Вакансия '{vacancy.Title}' заархивирована",
                userId: userId
            );*/

            return Ok(new { message = "Вакансия успешно заархивирована" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при архивации вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Разархивировать вакансию
    /// </summary>
    [HttpPost("{id:guid}/unarchive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnarchiveVacancy(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var vacancy = await _vacancyService.GetVacancyByIdAsync(id);

            if (vacancy == null)
                return NotFound(new { message = "Вакансия не найдена" });

            if (!vacancy.IsArchived)
                return BadRequest(new { message = "Вакансия не находится в архиве" });

            await _vacancyService.UnarchiveVacancyAsync(id, userId);

            // Логируем разархивацию
            /*await _auditService.LogAsync(
                entityName: "Vacancy",
                entityId: id,
                action: "Unarchive",
                description: $"Вакансия '{vacancy.Title}' разархивирована",
                userId: userId
            );*/

            return Ok(new { message = "Вакансия успешно разархивирована" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при разархивации вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить список компетенций для вакансии
    /// </summary>
    [HttpGet("{id:guid}/competencies")]
    [ProducesResponseType(typeof(List<CompetencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVacancyCompetencies(Guid id)
    {
        var competencies = await _vacancyService.GetVacancyCompetenciesAsync(id);
        return Ok(competencies);
    }

    /// <summary>
    /// Добавить компетенцию к вакансии
    /// </summary>
    [HttpPost("{id:guid}/competencies")]
    [ProducesResponseType(typeof(CompetencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCompetencyToVacancy(
        Guid id,
        [FromBody] AddCompetencyToVacancy request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var competency = await _vacancyService.AddCompetencyToVacancyAsync(id, request, userId);

            if (competency == null)
                return NotFound(new { message = "Вакансия не найдена" });

            return CreatedAtAction(nameof(GetVacancyCompetencies), new { id }, competency);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить компетенцию из вакансии
    /// </summary>
    [HttpDelete("{id:guid}/competencies/{competencyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCompetencyFromVacancy(Guid id, Guid competencyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _vacancyService.RemoveCompetencyFromVacancyAsync(id, competencyId, userId);

            if (!result)
                return NotFound(new { message = "Вакансия или компетенция не найдены" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении компетенции из вакансии");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Пользователь не авторизован");

        return Guid.Parse(userIdClaim);
    }
}
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
    private readonly ILogger<VacanciesController> _logger;

    public VacanciesController(IVacancyService vacancyService, ILogger<VacanciesController> logger)
    {
        _vacancyService = vacancyService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список вакансий
    /// </summary>
    /// <remarks>
    /// Поддерживает фильтрацию:
    /// - по поиску (search)
    /// - по отделу (department)
    /// - по архиву (isArchived)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacancies(
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] bool? isArchived = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetVacanciesQuery
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                Department = department,
                IsArchived = isArchived
            };

            var (items, totalCount) = await _vacancyService.GetVacanciesAsync(query);

            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка вакансий");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить карточку вакансии
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacancyById(Guid id)
    {
        try
        {
            var vacancy = await _vacancyService.GetVacancyByIdAsync(id);

            if (vacancy == null)
                return NotFound(new { message = "Вакансия не найдена" });

            return Ok(vacancy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новую вакансию
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VacancyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVacancy([FromBody] CreateVacancyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var vacancy = await _vacancyService.CreateVacancyAsync(request);
            return CreatedAtAction(nameof(GetVacancyById), new { id = vacancy.Id }, vacancy);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateVacancy(Guid id, [FromBody] UpdateVacancyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var vacancy = await _vacancyService.UpdateVacancyAsync(id, request);
            return Ok(vacancy);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
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
            var result = await _vacancyService.ArchiveVacancyAsync(id);

            if (!result)
                return NotFound(new { message = "Вакансия не найдена" });

            return Ok(new { message = "Вакансия успешно архивирована" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при архивировании вакансии {VacancyId}", id);
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
            var result = await _vacancyService.UnarchiveVacancyAsync(id);

            if (!result)
                return NotFound(new { message = "Вакансия не найдена" });

            return Ok(new { message = "Вакансия успешно разархивирована" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при разархивировании вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить вакансию
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteVacancy(Guid id)
    {
        try
        {
            var result = await _vacancyService.DeleteVacancyAsync(id);

            if (!result)
                return NotFound(new { message = "Вакансия не найдена" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении вакансии {VacancyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
using HRBars.Application.DTOs.Competency;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/competencies")]
[Authorize]
public class CompetencyController : ControllerBase
{
    private readonly ICompetencyService _competencyService;
    private readonly ILogger<CompetencyController> _logger;

    public CompetencyController(
        ICompetencyService competencyService,
        ILogger<CompetencyController> logger)
    {
        _competencyService = competencyService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список компетенций
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCompetencies(
        [FromQuery] Guid? templateId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isArchived = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetCompetenciesQuery
            {
                Page = page,
                PageSize = pageSize,
                TemplateId = templateId,
                Category = category,
                Search = search,
                IsArchived = isArchived
            };

            var (items, totalCount) = await _competencyService.GetCompetenciesAsync(query);

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
            _logger.LogError(ex, "Ошибка при получении списка компетенций");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить компетенцию по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompetencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCompetencyById(Guid id)
    {
        try
        {
            var competency = await _competencyService.GetCompetencyByIdAsync(id);

            if (competency == null)
                return NotFound(new { message = "Компетенция не найдена" });

            return Ok(competency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении компетенции {CompetencyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новую компетенцию в матрице
    /// </summary>
    [HttpPost("templates/{templateId:guid}/competencies")]
    [ProducesResponseType(typeof(CompetencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCompetency(Guid templateId, [FromBody] CreateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var competency = await _competencyService.CreateCompetencyAsync(templateId, request);
            return CreatedAtAction(nameof(GetCompetencyById), new { id = competency.Id }, competency);
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
            _logger.LogError(ex, "Ошибка при создании компетенции");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать компетенцию
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompetencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCompetency(Guid id, [FromBody] UpdateCompetencyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var competency = await _competencyService.UpdateCompetencyAsync(id, request);
            return Ok(competency);
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
            _logger.LogError(ex, "Ошибка при обновлении компетенции {CompetencyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить компетенцию
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCompetency(Guid id)
    {
        try
        {
            var result = await _competencyService.DeleteCompetencyAsync(id);

            if (!result)
                return NotFound(new { message = "Компетенция не найдена" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении компетенции {CompetencyId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
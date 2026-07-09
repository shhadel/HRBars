using HRBars.Application.DTOs.WorkExperience;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/work-experiences")]
public class WorkExperienceController(
    IWorkExperienceService workExperienceService,
    ILogger<WorkExperienceController> logger)
    : ControllerBase
{
    private readonly IWorkExperienceService _workExperienceService = workExperienceService;
    private readonly ILogger<WorkExperienceController> _logger = logger;

    /// <summary>
    /// Добавить опыт работы кандидату
    /// </summary>
    /// <remarks>Добавляет новую запись об опыте работы для указанного кандидата</remarks>
    [HttpPost("candidates/{candidateId:guid}/work-experience")]
    [ProducesResponseType(typeof(WorkExperienceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddWorkExperience(Guid candidateId, [FromBody] CreateWorkExperienceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var workExperience = await _workExperienceService.AddWorkExperienceAsync(candidateId, request);
            return CreatedAtAction(nameof(GetWorkExperienceById), new { id = workExperience.Id }, workExperience);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении опыта работы для кандидата {CandidateId}", candidateId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать опыт работы
    /// </summary>
    /// <remarks>Обновляет существующую запись об опыте работы</remarks>
    [HttpPut("single/{id:guid}")]
    [ProducesResponseType(typeof(WorkExperienceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateWorkExperience(Guid id, [FromBody] UpdateWorkExperienceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var workExperience = await _workExperienceService.UpdateWorkExperienceAsync(id, request);
            return Ok(workExperience);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении опыта работы {WorkExperienceId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить опыт работы
    /// </summary>
    /// <remarks>Удаляет запись об опыте работы по идентификатору</remarks>
    [HttpDelete("single/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWorkExperience(Guid id)
    {
        try
        {
            var result = await _workExperienceService.DeleteWorkExperienceAsync(id);

            if (!result)
                return NotFound(new { message = "Опыт работы не найден" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении опыта работы {WorkExperienceId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить опыт работы по ID
    /// </summary>
    /// <remarks>Вспомогательный метод для получения созданной записи</remarks>
    [HttpGet("single/{id:guid}")]
    [ProducesResponseType(typeof(WorkExperienceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkExperienceById(Guid id)
    {
        var workExperience = await _workExperienceService.GetWorkExperienceByIdAsync(id);

        if (workExperience == null)
            return NotFound(new { message = "Опыт работы не найден" });

        return Ok(workExperience);
    }

    /// <summary>
    /// Получить весь опыт работы кандидата
    /// </summary>
    /// <remarks>Возвращает список всего опыта работы для указанного кандидата</remarks>
    [HttpGet("candidates/{candidateId:guid}/work-experiences")]
    [ProducesResponseType(typeof(List<WorkExperienceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkExperiencesByCandidateId(Guid candidateId)
    {
        try
        {
            var candidateExists = await _workExperienceService.CandidateExistsAsync(candidateId);
            if (!candidateExists)
                return NotFound(new { message = $"Кандидат с ID {candidateId} не найден" });

            var workExperiences = await _workExperienceService.GetWorkExperiencesByCandidateIdAsync(candidateId);
            return Ok(workExperiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении опыта работы кандидата {CandidateId}", candidateId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Поиск компаний (автоподсказки)
    /// </summary>
    /// <remarks>
    /// Возвращает список компаний, которые уже есть в БД,
    /// для автозаполнения при вводе
    /// </remarks>
    [HttpGet("search/companies")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCompanies([FromQuery] string query)
    {
        try
        {
            var results = await _workExperienceService.SearchCompaniesAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске компаний по запросу {Query}", query);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
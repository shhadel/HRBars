using HRBars.Application.DTOs.Education;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/educations")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;
    private readonly ILogger<EducationController> _logger;

    public EducationController(IEducationService educationService, ILogger<EducationController> logger)
    {
        _educationService = educationService;
        _logger = logger;
    }

    /// <summary>
    /// Добавить образование кандидату
    /// </summary>
    /// <remarks>Добавляет новую запись об образовании для указанного кандидата</remarks>
    [HttpPost("candidates/{candidateId:guid}/education")]
    [ProducesResponseType(typeof(EducationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddEducation(Guid candidateId, [FromBody] CreateEducationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var education = await _educationService.AddEducationAsync(candidateId, request);
            return CreatedAtAction(nameof(GetEducationById), new { id = education.Id }, education);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении образования для кандидата {CandidateId}", candidateId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать образование
    /// </summary>
    /// <remarks>Обновляет существующую запись об образовании</remarks>
    [HttpPut("single/{id:guid}")]
    [ProducesResponseType(typeof(EducationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEducation(Guid id, [FromBody] UpdateEducationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var education = await _educationService.UpdateEducationAsync(id, request);
            return Ok(education);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении образования {EducationId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить образование
    /// </summary>
    /// <remarks>Удаляет запись об образовании по идентификатору</remarks>
    [HttpDelete("single/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEducation(Guid id)
    {
        try
        {
            var result = await _educationService.DeleteEducationAsync(id);

            if (!result)
                return NotFound(new { message = "Образование не найдено" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении образования {EducationId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить образование по ID
    /// </summary>
    /// <remarks>Вспомогательный метод для получения созданной записи</remarks>
    [HttpGet("single/{id:guid}")]
    [ProducesResponseType(typeof(EducationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEducationById(Guid id)
    {
        var education = await _educationService.GetEducationByIdAsync(id);

        if (education == null)
            return NotFound(new { message = "Образование не найдено" });

        return Ok(education);
    }

    /// <summary>
    /// Получить все образования кандидата
    /// </summary>
    /// <remarks>Возвращает список всех образований для указанного кандидата</remarks>
    [HttpGet("candidates/{candidateId:guid}/educations")]
    [ProducesResponseType(typeof(List<EducationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEducationsByCandidateId(Guid candidateId)
    {
        try
        {
            var candidateExists = await _educationService.CandidateExistsAsync(candidateId);
            if (!candidateExists)
                return NotFound(new { message = $"Кандидат с ID {candidateId} не найден" });

            var educations = await _educationService.GetEducationsByCandidateIdAsync(candidateId);
            return Ok(educations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении образований кандидата {CandidateId}", candidateId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Поиск учебных заведений (автоподсказки)
    /// </summary>
    /// <remarks>
    /// Возвращает список учебных заведений, которые уже есть в БД,
    /// для автозаполнения при вводе
    /// </remarks>
    [HttpGet("search/institutions")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchInstitutions([FromQuery] string query)
    {
        try
        {
            var results = await _educationService.SearchInstitutionsAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске учебных заведений по запросу {Query}", query);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
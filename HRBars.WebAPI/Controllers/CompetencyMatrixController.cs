using HRBars.Application.DTOs.CompetencyMatrix;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/competency-matrices")]
[Authorize]
public class CompetencyMatrixController : ControllerBase
{
    private readonly ICompetencyMatrixService _competencyMatrixService;
    private readonly ILogger<CompetencyMatrixController> _logger;

    public CompetencyMatrixController(
        ICompetencyMatrixService competencyMatrixService,
        ILogger<CompetencyMatrixController> logger)
    {
        _competencyMatrixService = competencyMatrixService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список матриц компетенций
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCompetencyMatrices(
        [FromQuery] Guid? vacancyId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isArchived = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetCompetencyMatricesQuery
            {
                Page = page,
                PageSize = pageSize,
                VacancyId = vacancyId,
                Search = search,
                IsArchived = isArchived
            };

            var (items, totalCount) = await _competencyMatrixService.GetCompetencyMatricesAsync(query);

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
            _logger.LogError(ex, "Ошибка при получении списка матриц компетенций");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить матрицу компетенций по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompetencyMatrixResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCompetencyMatrixById(Guid id)
    {
        try
        {
            var matrix = await _competencyMatrixService.GetCompetencyMatrixByIdAsync(id);

            if (matrix == null)
                return NotFound(new { message = "Матрица компетенций не найдена" });

            return Ok(matrix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении матрицы компетенций {MatrixId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Создать новую матрицу компетенций
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompetencyMatrixResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCompetencyMatrix([FromBody] CreateCompetencyMatrixRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var matrix = await _competencyMatrixService.CreateCompetencyMatrixAsync(request);
            return CreatedAtAction(nameof(GetCompetencyMatrixById), new { id = matrix.Id }, matrix);
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
            _logger.LogError(ex, "Ошибка при создании матрицы компетенций");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Редактировать матрицу компетенций
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompetencyMatrixResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCompetencyMatrix(Guid id, [FromBody] UpdateCompetencyMatrixRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var matrix = await _competencyMatrixService.UpdateCompetencyMatrixAsync(id, request);
            return Ok(matrix);
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
            _logger.LogError(ex, "Ошибка при обновлении матрицы компетенций {MatrixId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Удалить матрицу компетенций
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCompetencyMatrix(Guid id)
    {
        try
        {
            var result = await _competencyMatrixService.DeleteCompetencyMatrixAsync(id);

            if (!result)
                return NotFound(new { message = "Матрица компетенций не найдена" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении матрицы компетенций {MatrixId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
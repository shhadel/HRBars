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
    /// Получить список матриц
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMatrixList()
    {
        try
        {
            var matrixListResponses = await _competencyMatrixService.GetMatrixByIdAsync();

            return Ok(matrixListResponses);
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
}
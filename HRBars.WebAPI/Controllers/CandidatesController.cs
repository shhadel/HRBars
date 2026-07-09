using HRBars.Application.DTOs.Application;
using HRBars.Application.DTOs.Candidate;
using HRBars.Application.Interfaces;
using HRBars.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    [HttpGet]
    [RequirePermission("candidates.view")]
    public async Task<ActionResult> GetCandidates([FromQuery] GetCandidates query)
    {
        var result = await _candidateService.GetCandidatesAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("candidates.view")]
    public async Task<ActionResult> GetCandidate(Guid id)
    {
        var candidate = await _candidateService.GetCandidateByIdAsync(id);

        if (candidate == null)
            return NotFound();

        return Ok(candidate);
    }

    [HttpPost]
    [RequirePermission("candidates.create")]
    public async Task<ActionResult> CreateCandidate(CreateCandidate request)
    {
        try
        {
            var candidate = await _candidateService.CreateCandidateAsync(request);

            return CreatedAtAction(
                nameof(GetCandidate),
                new { id = candidate.Id },
                candidate);
        }

        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("candidates.edit")]
    public async Task<ActionResult> UpdateCandidate(Guid id, UpdateCandidate request)
    {
        try
        {
            var candidate = await _candidateService.UpdateCandidateAsync(id, request);

            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("candidates.archive")]
    public async Task<ActionResult> ArchiveCandidate(Guid id)
    {
        var archived = await _candidateService.ArchiveCandidateAsync(id);

        if (!archived)
            return NotFound();

        return NoContent();
    }

    [HttpPut("restore/{id:guid}")]
    [RequirePermission("candidates.archive")]
    public async Task<ActionResult> RestoreCandidate(Guid id)
    {
        var archived = await _candidateService.RestoreCandidateAsync(id);

        if (!archived)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:guid}/applications")]
    [RequirePermission("candidates.view")]
    [ProducesResponseType(typeof(List<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetCandidateApplications(Guid id)
    {
        try
        {
            var applications = await _candidateService.GetCandidateApplicationAsync(id);

            if (applications == null || !applications.Any())
                return NotFound(new { message = $"Активные заявки для кандидата {id} не найдены" });

            return Ok(new
            {
                candidateId = id,
                count = applications.Count,
                items = applications
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}
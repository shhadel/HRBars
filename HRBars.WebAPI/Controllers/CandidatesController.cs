using HRBars.Application.DTOs.Candidate;
using HRBars.Application.Interfaces;
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
	public async Task<ActionResult> GetCandidates([FromQuery] GetCandidates query)
	{
		var result = await _candidateService.GetCandidatesAsync(query);
		return Ok(result);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult> GetCandidate(Guid id)
	{
		var candidate = await _candidateService.GetCandidateByIdAsync(id);

		if (candidate == null)
			return NotFound();

		return Ok(candidate);
	}

	[HttpPost]
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
	public async Task<ActionResult> ArchiveCandidate(Guid id)
	{
		var archived = await _candidateService.ArchiveCandidateAsync(id);

		if (!archived)
			return NotFound();

		return NoContent();
	}
}
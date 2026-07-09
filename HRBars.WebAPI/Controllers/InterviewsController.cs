using HRBars.Application.DTOs.Interview;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using HRBars.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewsController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    [HttpGet]
    [RequirePermission("interviews.view")]
    public async Task<ActionResult<PaginatedResult<InterviewResponse>>> GetInterviews(
        [FromQuery] GetInterviews query)
    {
        var result = await _interviewService.GetInterviewsAsync(query);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("interviews.view")]
    public async Task<ActionResult<InterviewDetails>> GetInterview(Guid id)
    {
        var interview = await _interviewService.GetInterviewByIdAsync(id);

        if (interview == null)
            return NotFound();

        return Ok(interview);
    }

    [HttpPost]
    [RequirePermission("interviews.create")]
    public async Task<ActionResult<InterviewResponse>> CreateInterview(
        CreateInterview request)
    {
        var interview = await _interviewService.CreateInterviewAsync(request);

        return CreatedAtAction(
            nameof(GetInterview),
            new { id = interview.Id },
            interview);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("interviews.edit")]
    public async Task<ActionResult<InterviewResponse>> UpdateInterview(
        Guid id,
        UpdateInterview request)
    {
        var interview = await _interviewService.UpdateInterviewAsync(id, request);

        if (interview == null)
            return NotFound();

        return Ok(interview);
    }

    [HttpPatch("{id:guid}/decision")]
    [RequirePermission("interviews.change_status")]
    public async Task<ActionResult<InterviewResponse>> MakeDecision(
        Guid id,
        MakeDecision request)
    {
        var interview = await _interviewService.MakeDecisionAsync(id, request);

        if (interview == null)
            return NotFound();

        return Ok(interview);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("interviews.delete")]
    public async Task<IActionResult> ArchiveInterview(Guid id)
    {
        var archived = await _interviewService.ArchiveInterviewAsync(id);

        if (!archived)
            return NotFound();

        return NoContent();
    }
}
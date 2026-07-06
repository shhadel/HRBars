using HRBars.Application.DTOs.InterviewScore;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/interviews/{interviewId:guid}/scores")]
[Authorize]
public class InterviewCompetencyScoresController : ControllerBase
{
    private readonly IInterviewCompetencyScoreService _service;

    public InterviewCompetencyScoresController(
        IInterviewCompetencyScoreService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<InterviewCompetencyScoreResponse>>> GetScores(
        Guid interviewId)
    {
        var scores = await _service.GetScoresAsync(interviewId);

        return Ok(scores);
    }

    [HttpPost]
    public async Task<ActionResult<List<InterviewCompetencyScoreResponse>>> SaveScores(
        Guid interviewId,
        SaveInterviewScoresRequest request)
    {
        var scores = await _service.SaveScoresAsync(interviewId, request);

        return Ok(scores);
    }
}
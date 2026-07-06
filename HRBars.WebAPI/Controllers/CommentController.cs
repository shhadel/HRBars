using HRBars.Application.DTOs.Comment;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("{interviewId:guid}")]
    public async Task<ActionResult<List<CommentResponse>>> GetComments(Guid interviewId)
    {
        var comments = await _commentService.GetCommentsAsync(interviewId);

        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> CreateComment(CreateComment request)
    {
        var comment = await _commentService.CreateCommentAsync(request);

        return CreatedAtAction(
            nameof(GetComments),
            new { interviewId = request.InterviewId },
            comment);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> UpdateComment(
        Guid id,
        UpdateComment request)
    {
        var comment = await _commentService.UpdateCommentAsync(id, request);

        if (comment == null)
            return NotFound();

        return Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var deleted = await _commentService.DeleteCommentAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
using HRBars.Application.DTOs.Comment;

namespace HRBars.Application.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponse>> GetCommentsAsync(Guid interviewId);

    Task<CommentResponse> CreateCommentAsync(CreateComment request);

    Task<CommentResponse?> UpdateCommentAsync(Guid id, UpdateComment request);

    Task<bool> DeleteCommentAsync(Guid id);
}
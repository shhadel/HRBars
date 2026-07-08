using HRBars.Application.DTOs.Comment;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRBars.Application.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CommentService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CommentResponse>> GetCommentsAsync(Guid interviewId)
    {
        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.InterviewId == interviewId)
            .Include(c => c.CreatedByUser)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse
            {
                Id = c.Id,
                Section = c.Section,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                CreatedBy = BuildFullName(
                    c.CreatedByUser.LastName,
                    c.CreatedByUser.FirstName,
                    c.CreatedByUser.MiddleName)
            })
            .ToListAsync();
    }

    public async Task<CommentResponse> CreateCommentAsync(CreateComment request)
    {
        var interviewExists = await _context.Interviews
            .AnyAsync(i => i.Id == request.InterviewId);

        if (!interviewExists)
            throw new KeyNotFoundException("Собеседование не найдено.");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            InterviewId = request.InterviewId,
            Section = request.Section?.Trim(),
            Text = request.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Пользователь не авторизован")
        };

        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var created = await _context.Comments
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .FirstAsync(c => c.Id == comment.Id);

        return new CommentResponse
        {
            Id = created.Id,
            Section = created.Section,
            Text = created.Text,
            CreatedAt = created.CreatedAt,
            CreatedBy = BuildFullName(
                created.CreatedByUser.LastName,
                created.CreatedByUser.FirstName,
                created.CreatedByUser.MiddleName)
        };
    }

    public async Task<CommentResponse?> UpdateCommentAsync(
    Guid id,
    UpdateComment request)
    {
        var comment = await _context.Comments
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            return null;

        comment.Section = request.Section?.Trim();
        comment.Text = request.Text.Trim();

        await _context.SaveChangesAsync();

        return new CommentResponse
        {
            Id = comment.Id,
            Section = comment.Section,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            CreatedBy = BuildFullName(
                comment.CreatedByUser.LastName,
                comment.CreatedByUser.FirstName,
                comment.CreatedByUser.MiddleName)
        };
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            return false;

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();

        return true;
    }

    private static string BuildFullName(
    string lastName,
    string firstName,
    string? middleName)
    {
        return string.Join(" ",
            new[]
            {
            lastName,
            firstName,
            middleName
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
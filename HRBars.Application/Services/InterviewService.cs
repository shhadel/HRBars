using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Interview;
using HRBars.Application.DTOs.Application;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Domain.Enums;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRBars.Application.Services;

public class InterviewService : IInterviewService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public InterviewService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<InterviewResponse>> GetInterviewsAsync(GetInterviews query)
    {
        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        var interviewsQuery = _context.Interviews
            .AsNoTracking()
            .Where(i => i.ArchivedAt == null)
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Vacancy)
            .Include(i => i.CreatedByUser)
            .AsQueryable();

        if (query.CandidateId.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.Application.CandidateId == query.CandidateId.Value);
        }

        if (query.VacancyId.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.Application.VacancyId == query.VacancyId.Value);
        }

        if (query.Status.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.Status == (short)query.Status.Value);
        }

        if (query.Result.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.Result == (short)query.Result.Value);
        }

        if (query.DateFrom.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.InterviewDate >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            interviewsQuery = interviewsQuery.Where(i =>
                i.InterviewDate <= query.DateTo.Value);
        }

        var totalCount = await interviewsQuery.CountAsync();

        var items = await interviewsQuery
            .OrderBy(i => i.InterviewDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InterviewResponse
            {
                Id = i.Id,
                InterviewDate = i.InterviewDate,
                Status = (InterviewStatus)i.Status,
                Format = (InterviewFormat)i.Format,
                Result = i.Result.HasValue 
                ? (InterviewResult?)i.Result.Value
                : null,

                CandidateName = string.Join(" ",
                    new[]
                    {
                    i.Application.Candidate.LastName,
                    i.Application.Candidate.FirstName,
                    i.Application.Candidate.MiddleName
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),

                VacancyTitle = i.Application.Vacancy.Title,

                CreatedBy = string.Join(" ",
                    new[]
                    {
                    i.CreatedByUser.LastName,
                    i.CreatedByUser.FirstName,
                    i.CreatedByUser.MiddleName
                    }.Where(x => !string.IsNullOrWhiteSpace(x)))
            })
            .ToListAsync();

        return new PaginatedResult<InterviewResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<InterviewDetails?> GetInterviewByIdAsync(Guid id)
    {
        var interview = await _context.Interviews
            .AsNoTracking()
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Vacancy)
            .Include(i => i.CreatedByUser)
            .Include(i => i.DecidedByUser)
            .FirstOrDefaultAsync(i =>
                i.Id == id &&
                i.ArchivedAt == null);

        if (interview == null)
            return null;

        return new InterviewDetails
        {
            Id = interview.Id,

            InterviewDate = interview.InterviewDate,

            Format = (InterviewFormat)interview.Format,

            Status = (InterviewStatus)interview.Status,

            Result = interview.Result.HasValue 
            ? (InterviewResult?)interview.Result.Value 
            : null,

            CandidateName = BuildFullName(
                interview.Application.Candidate.LastName,
                interview.Application.Candidate.FirstName,
                interview.Application.Candidate.MiddleName),

            VacancyTitle = interview.Application.Vacancy.Title,

            CreatedBy = BuildFullName(
                interview.CreatedByUser.LastName,
                interview.CreatedByUser.FirstName,
                interview.CreatedByUser.MiddleName),

            DurationMinutes = interview.DurationMinutes,

            Location = interview.Location,

            Plan = interview.Plan,

            DecisionComment = interview.DecisionComment,

            DecisionDate = interview.DecisionDate,

            DecidedBy = interview.DecidedByUser == null
                ? null
                : BuildFullName(
                    interview.DecidedByUser.LastName,
                    interview.DecidedByUser.FirstName,
                    interview.DecidedByUser.MiddleName)
        };
    }

    public async Task<InterviewResponse> CreateInterviewAsync(CreateInterview request)
    {
        var application = await _context.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId);

        if (application == null)
            throw new KeyNotFoundException("Заявка не найдена.");

        if (application.Status == (short)ApplicationStatus.Rejected ||
            application.Status == (short)ApplicationStatus.Cancelled ||
            application.Status == (short)ApplicationStatus.Hired)
        {
            throw new InvalidOperationException("Нельзя назначить собеседование для заявки с текущим статусом.");
        }

        var now = DateTime.UtcNow;

        var interview = new Interview
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            InterviewDate = request.InterviewDate,
            Format = (short)request.Format,
            Status = (short)InterviewStatus.Scheduled,
            DurationMinutes = request.DurationMinutes,
            Location = request.Location?.Trim(),
            Plan = request.Plan?.Trim(),
            CreatedAt = now,
            CreatedByUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Пользователь не авторизован")
        };

        application.Status = (short)ApplicationStatus.InterviewScheduled;

        var statusHistory = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Status = (short)ApplicationStatus.InterviewScheduled,
            Comment = "Назначено собеседование",
            ChangedAt = now
        };

        await _context.Interviews.AddAsync(interview);
        await _context.ApplicationStatusHistories.AddAsync(statusHistory);

        await _context.SaveChangesAsync();

        return new InterviewResponse
        {
            Id = interview.Id,
            InterviewDate = interview.InterviewDate,
            Format = (InterviewFormat)interview.Format,
            Status = (InterviewStatus)interview.Status,
            Result = interview.Result.HasValue 
            ? (InterviewResult?)interview.Result.Value
            : null,

            CandidateName = BuildFullName(
                application.Candidate.LastName,
                application.Candidate.FirstName,
                application.Candidate.MiddleName),

            VacancyTitle = application.Vacancy.Title,

            CreatedBy = string.Empty
        };
    }

    public async Task<InterviewResponse?> UpdateInterviewAsync(Guid id, UpdateInterview request)
    {
        var interview = await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Vacancy)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i =>
                i.Id == id &&
                i.ArchivedAt == null);

        if (interview == null)
            return null;

        if (interview.Status == (short)InterviewStatus.Completed)
            throw new InvalidOperationException("Нельзя изменить завершенное собеседование.");

        interview.InterviewDate = request.InterviewDate;
        interview.Format = (short)request.Format;
        interview.Status = (short)request.Status;
        interview.DurationMinutes = request.DurationMinutes;
        interview.Location = request.Location?.Trim();
        interview.Plan = request.Plan?.Trim();

        await _context.SaveChangesAsync();

        return new InterviewResponse
        {
            Id = interview.Id,
            InterviewDate = interview.InterviewDate,
            Format = (InterviewFormat)interview.Format,
            Status = (InterviewStatus)interview.Status,
            Result = interview.Result.HasValue 
            ? (InterviewResult?)interview.Result.Value 
            : null,

            CandidateName = BuildFullName(
                interview.Application.Candidate.LastName,
                interview.Application.Candidate.FirstName,
                interview.Application.Candidate.MiddleName),

            VacancyTitle = interview.Application.Vacancy.Title,

            CreatedBy = BuildFullName(
                interview.CreatedByUser.LastName,
                interview.CreatedByUser.FirstName,
                interview.CreatedByUser.MiddleName)
        };
    }

    public async Task<InterviewResponse?> MakeDecisionAsync(Guid id, MakeDecision request)
    {
        var interview = await _context.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Vacancy)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i =>
                i.Id == id &&
                i.ArchivedAt == null);

        if (interview == null)
            return null;

        if (interview.Result.HasValue)
            throw new InvalidOperationException("Решение по собеседованию уже принято.");

        if (interview.Status == (short)InterviewStatus.Cancelled)
            throw new InvalidOperationException("Нельзя принять решение по отмененному собеседованию.");

        if (interview.Application.Status != (short)ApplicationStatus.InterviewScheduled)
        {
            throw new InvalidOperationException(
                "Статус заявки не позволяет принять решение по собеседованию.");
        }

        var now = DateTime.UtcNow;

        interview.Result = (short)request.Result;
        interview.DecisionComment = request.DecisionComment?.Trim();
        interview.DecisionDate = now;
        interview.DecidedByUserId = _currentUser.UserId;
        interview.Status = (short)InterviewStatus.Completed;

        interview.Application.Status = (short)GetApplicationStatus(request.Result);

        var statusHistory = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = interview.Application.Id,
            Status = interview.Application.Status,
            Comment = $"Результат собеседования: {request.Result}",
            ChangedAt = now
        };

        await _context.ApplicationStatusHistories.AddAsync(statusHistory);

        await _context.SaveChangesAsync();

        return new InterviewResponse
        {
            Id = interview.Id,
            InterviewDate = interview.InterviewDate,
            Format = (InterviewFormat)interview.Format,
            Status = (InterviewStatus)interview.Status,
            Result = interview.Result.HasValue
            ? (InterviewResult?)interview.Result.Value
            : null,

            CandidateName = BuildFullName(
                interview.Application.Candidate.LastName,
                interview.Application.Candidate.FirstName,
                interview.Application.Candidate.MiddleName),

            VacancyTitle = interview.Application.Vacancy.Title,

            CreatedBy = BuildFullName(
                interview.CreatedByUser.LastName,
                interview.CreatedByUser.FirstName,
                interview.CreatedByUser.MiddleName)
        };
    }

    public async Task<bool> ArchiveInterviewAsync(Guid id)
    {
        var interview = await GetActiveInterviewAsync(id);

        if (interview == null)
            return false;

        if (interview.Status != (short)InterviewStatus.Completed && 
            interview.Status != (short)InterviewStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Архивировать можно только завершенное или отмененное собеседование.");
        }

        interview.ArchivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private Task<Interview?> GetActiveInterviewAsync(Guid id)
    {
        return _context.Interviews
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i =>
                i.Id == id &&
                i.ArchivedAt == null);
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

    private static ApplicationStatus GetApplicationStatus(InterviewResult result)
    {
        return result switch
        {
            InterviewResult.Passed => ApplicationStatus.InterviewPassed,
            InterviewResult.Failed => ApplicationStatus.InterviewFailed,
            _ => throw new InvalidOperationException("Неизвестный результат собеседования.")
        };
    }
}
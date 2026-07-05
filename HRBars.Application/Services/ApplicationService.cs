using HRBars.Application.DTOs.Application;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(AppDbContext context, ILogger<ApplicationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<ApplicationListResponse> Items, int TotalCount)> GetApplicationsAsync(GetApplicationsQuery query)
    {
        var applicationsQuery = _context.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .Include(a => a.CreatedByUser)
            .AsQueryable();

        if (query.CandidateId.HasValue)
        {
            applicationsQuery = applicationsQuery.Where(a => a.CandidateId == query.CandidateId.Value);
        }

        if (query.VacancyId.HasValue)
        {
            applicationsQuery = applicationsQuery.Where(a => a.VacancyId == query.VacancyId.Value);
        }

        if (query.Status.HasValue)
        {
            applicationsQuery = applicationsQuery.Where(a => a.Status == (short)query.Status.Value);
        }

        var totalCount = await applicationsQuery.CountAsync();

        var items = await applicationsQuery
            .OrderByDescending(a => a.AppliedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new ApplicationListResponse
            {
                Id = a.Id,
                Status = (ApplicationStatus)a.Status,
                StatusName = GetStatusName((ApplicationStatus)a.Status),
                AppliedAt = a.AppliedAt,
                ClosedAt = a.ClosedAt,
                CandidateName = $"{a.Candidate.LastName} {a.Candidate.FirstName} {a.Candidate.MiddleName}".Trim(),
                VacancyTitle = a.Vacancy.Title,
                CreatedByUserName = $"{a.CreatedByUser.LastName} {a.CreatedByUser.FirstName}".Trim()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ApplicationResponse?> GetApplicationByIdAsync(Guid id)
    {
        var application = await _context.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .Include(a => a.CreatedByUser)
            .Include(a => a.StatusHistories)
            .Include(a => a.Interviews)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return null;

        return MapToApplicationResponse(application);
    }

    public async Task<ApplicationResponse> CreateApplicationAsync(CreateApplicationRequest request, Guid createdByUserId)
    {
        var candidateExists = await _context.Candidates.AnyAsync(c => c.Id == request.CandidateId);
        if (!candidateExists)
            throw new KeyNotFoundException($"Кандидат с ID {request.CandidateId} не найден");

        var vacancyExists = await _context.Vacancies.AnyAsync(v => v.Id == request.VacancyId);
        if (!vacancyExists)
            throw new KeyNotFoundException($"Вакансия с ID {request.VacancyId} не найдена");

        var userExists = await _context.Users.AnyAsync(u => u.Id == createdByUserId);
        if (!userExists)
            throw new KeyNotFoundException($"Пользователь с ID {createdByUserId} не найден");

        var existingApplication = await _context.Applications
            .AnyAsync(a => a.CandidateId == request.CandidateId && a.VacancyId == request.VacancyId);
        if (existingApplication)
            throw new InvalidOperationException("Заявка на эту вакансию от этого кандидата уже существует");

        var application = new Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            Status = (short)request.Status,
            AppliedAt = DateTime.UtcNow,
            CandidateId = request.CandidateId,
            VacancyId = request.VacancyId,
            CreatedByUserId = createdByUserId
        };

        var statusHistory = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Status = (short)request.Status,
            Comment = request.Comment ?? $"Заявка создана со статусом {GetStatusName(request.Status)}",
            ChangedAt = DateTime.UtcNow
        };

        await _context.Applications.AddAsync(application);
        await _context.ApplicationStatusHistories.AddAsync(statusHistory);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана заявка {ApplicationId} для кандидата {CandidateId} на вакансию {VacancyId}",
            application.Id, request.CandidateId, request.VacancyId);

        return await GetApplicationByIdAsync(application.Id)
            ?? throw new Exception("Не удалось получить созданную заявку");
    }

    public async Task<ApplicationResponse> UpdateApplicationAsync(Guid id, UpdateApplicationRequest request)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            throw new KeyNotFoundException($"Заявка с ID {id} не найдена");

        // Проверка существования кандидата
        var candidateExists = await _context.Candidates.AnyAsync(c => c.Id == request.CandidateId);
        if (!candidateExists)
            throw new KeyNotFoundException($"Кандидат с ID {request.CandidateId} не найден");

        // Проверка существования вакансии
        var vacancyExists = await _context.Vacancies.AnyAsync(v => v.Id == request.VacancyId);
        if (!vacancyExists)
            throw new KeyNotFoundException($"Вакансия с ID {request.VacancyId} не найдена");

        // Проверка на дубликат заявки (исключая текущую)
        var existingApplication = await _context.Applications
            .AnyAsync(a => a.CandidateId == request.CandidateId && a.VacancyId == request.VacancyId && a.Id != id);
        if (existingApplication)
            throw new InvalidOperationException("Заявка на эту вакансию от этого кандидата уже существует");

        // Если статус меняется, добавляем запись в историю
        if (application.Status != (short)request.Status)
        {
            var statusHistory = new ApplicationStatusHistory
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                Status = (short)request.Status,
                Comment = $"Статус изменен с {GetStatusName((ApplicationStatus)application.Status)} на {GetStatusName(request.Status)}",
                ChangedAt = DateTime.UtcNow
            };
            await _context.ApplicationStatusHistories.AddAsync(statusHistory);
        }

        application.CandidateId = request.CandidateId;
        application.VacancyId = request.VacancyId;
        application.Status = (short)request.Status;

        // Если статус "Hired" или "Rejected" - закрываем заявку
        if (request.Status == ApplicationStatus.Hired || request.Status == ApplicationStatus.Rejected)
        {
            application.ClosedAt = DateTime.UtcNow;
        }
        else if (application.ClosedAt.HasValue)
        {
            // Если заявка была закрыта, а статус изменился на другой - открываем заново
            application.ClosedAt = null;
        }

        _context.Applications.Update(application);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлена заявка {ApplicationId}", id);

        return await GetApplicationByIdAsync(id)
            ?? throw new Exception("Не удалось получить обновленную заявку");
    }

    public async Task<ApplicationResponse> ChangeStatusAsync(Guid id, ChangeStatusRequest request)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            throw new KeyNotFoundException($"Заявка с ID {id} не найдена");

        if (application.Status == (short)request.Status)
            throw new InvalidOperationException($"Заявка уже имеет статус {GetStatusName(request.Status)}");

        var statusHistory = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Status = (short)request.Status,
            Comment = request.Comment ?? $"Статус изменен на {GetStatusName(request.Status)}",
            ChangedAt = DateTime.UtcNow
        };

        application.Status = (short)request.Status;

        if (request.Status == ApplicationStatus.Hired || request.Status == ApplicationStatus.Rejected)
        {
            application.ClosedAt = DateTime.UtcNow;
        }
        else if (application.ClosedAt.HasValue)
        {
            application.ClosedAt = null;
        }

        await _context.ApplicationStatusHistories.AddAsync(statusHistory);
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Изменен статус заявки {ApplicationId} на {Status}", id, request.Status);

        return await GetApplicationByIdAsync(id)
            ?? throw new Exception("Не удалось получить обновленную заявку");
    }

    public async Task<bool> DeleteApplicationAsync(Guid id)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null)
            return false;

        _context.Applications.Remove(application);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалена заявка {ApplicationId}", id);

        return true;
    }

    public async Task<bool> ApplicationExistsAsync(Guid id)
    {
        return await _context.Applications.AnyAsync(a => a.Id == id);
    }

    public async Task<bool> CandidateExistsAsync(Guid candidateId)
    {
        return await _context.Candidates.AnyAsync(c => c.Id == candidateId);
    }

    public async Task<bool> VacancyExistsAsync(Guid vacancyId)
    {
        return await _context.Vacancies.AnyAsync(v => v.Id == vacancyId);
    }

    #region Private Helper Methods

    private ApplicationResponse MapToApplicationResponse(Domain.Entities.Application application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            Status = (ApplicationStatus)application.Status,
            StatusName = GetStatusName((ApplicationStatus)application.Status),
            AppliedAt = application.AppliedAt,
            ClosedAt = application.ClosedAt,
            Candidate = new ApplicationCandidateDto
            {
                Id = application.Candidate.Id,
                FirstName = application.Candidate.FirstName,
                LastName = application.Candidate.LastName,
                MiddleName = application.Candidate.MiddleName,
                FullName = $"{application.Candidate.LastName} {application.Candidate.FirstName} {application.Candidate.MiddleName}".Trim(),
                Phone = application.Candidate.Phone,
                Email = application.Candidate.Email,
                City = application.Candidate.City,
                Skills = application.Candidate.Skills
            },
            Vacancy = new ApplicationVacancyDto
            {
                Id = application.Vacancy.Id,
                Title = application.Vacancy.Title,
                Department = application.Vacancy.Department,
                Description = application.Vacancy.Description
            },
            CreatedByUser = new ApplicationUserDto
            {
                Id = application.CreatedByUser.Id,
                FirstName = application.CreatedByUser.FirstName,
                LastName = application.CreatedByUser.LastName,
                MiddleName = application.CreatedByUser.MiddleName,
                FullName = $"{application.CreatedByUser.LastName} {application.CreatedByUser.FirstName} {application.CreatedByUser.MiddleName}".Trim(),
                Email = application.CreatedByUser.Email
            },
            StatusHistories = application.StatusHistories
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new ApplicationStatusHistoryDto
                {
                    Id = h.Id,
                    Status = (ApplicationStatus)h.Status,
                    StatusName = GetStatusName((ApplicationStatus)h.Status),
                    Comment = h.Comment,
                    ChangedAt = h.ChangedAt
                })
                .ToList(),
            Interviews = application.Interviews
                .Select(i => new ApplicationInterviewDto
                {
                    Id = i.Id,
                    InterviewDate = i.InterviewDate,
                    Format = GetInterviewFormat(i.Format),
                    Status = GetInterviewStatus(i.Status),
                    DurationMinutes = i.DurationMinutes,
                    Location = i.Location,
                    Plan = i.Plan,
                    Result = i.Result.HasValue ? GetInterviewResult(i.Result.Value) : null,
                    DecisionComment = i.DecisionComment,
                    DecisionDate = i.DecisionDate
                })
                .ToList()
        };
    }

    private static string GetStatusName(ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.New => "Новая",
            ApplicationStatus.InReview => "На рассмотрении",
            ApplicationStatus.InterviewScheduled => "Собеседование назначено",
            ApplicationStatus.InterviewPassed => "Собеседование пройдено",
            ApplicationStatus.InterviewFailed => "Собеседование провалено",
            ApplicationStatus.OfferExtended => "Оффер направлен",
            ApplicationStatus.OfferAccepted => "Оффер принят",
            ApplicationStatus.OfferDeclined => "Оффер отклонен",
            ApplicationStatus.Hired => "Принят на работу",
            ApplicationStatus.Rejected => "Отклонен",
            ApplicationStatus.Cancelled => "Отменен",
            _ => status.ToString()
        };
    }

    private string GetInterviewFormat(short format)
    {
        return format switch
        {
            1 => "Очное",
            2 => "Онлайн",
            3 => "Телефонное",
            _ => "Не указано"
        };
    }

    private string GetInterviewStatus(short status)
    {
        return status switch
        {
            1 => "Запланировано",
            2 => "Проведено",
            3 => "Отменено",
            4 => "Перенесено",
            _ => "Не указано"
        };
    }

    private string GetInterviewResult(short result)
    {
        return result switch
        {
            1 => "Принят",
            2 => "Отклонен",
            3 => "На рассмотрении",
            _ => "Не указано"
        };
    }

    #endregion
}
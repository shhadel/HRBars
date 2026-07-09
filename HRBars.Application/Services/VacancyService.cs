using HRBars.Application.DTOs.Candidate;
using HRBars.Application.DTOs.Competency;
using HRBars.Application.DTOs.CompetencyMatrix;
using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Vacancy;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Domain.Enums;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class VacancyService : IVacancyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VacancyService> _logger;
    private readonly ICurrentUserService _currentUser;

    public VacancyService(AppDbContext context, ILogger<VacancyService> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<(List<VacancyResponse> Items, int TotalCount)> GetVacanciesAsync(GetVacanciesQuery query)
    {
        var vacanciesQuery = _context.Vacancies
            .Include(v => v.Applications)
                .ThenInclude(x => x.Interviews)
            .Include(v => v.CreatedByUser) // ✅ Добавляем Include для CreatedByUser
            .Include(v => v.UpdatedByUser) // ✅ Добавляем Include для UpdatedByUser
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            vacanciesQuery = vacanciesQuery.Where(v =>
                v.Title.ToLower().Contains(search) ||
                (v.Department != null && v.Department.ToLower().Contains(search)) ||
                (v.Description != null && v.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            vacanciesQuery = vacanciesQuery.Where(v => v.Department == query.Department);
        }

        if (query.IsArchived.HasValue)
        {
            vacanciesQuery = vacanciesQuery.Where(v => v.IsArchived == query.IsArchived.Value);
        }

        var totalCount = await vacanciesQuery.CountAsync();

        var vacancies = await vacanciesQuery
            .OrderByDescending(v => v.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(v => new VacancyResponse
            {
                Id = v.Id,
                Title = v.Title,
                Department = v.Department,
                Description = v.Description,
                SalaryFrom = v.SalaryFrom,
                SalaryTo = v.SalaryTo,
                ExperienceRequired = (ExperienceLevel)v.ExperienceRequired,
                EmploymentType = (EmploymentType)v.EmploymentType,
                IsArchived = v.IsArchived,
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedByUser != null
                    ? $"{v.CreatedByUser.FirstName} {v.CreatedByUser.LastName}"
                    : "Unknown",
                ModifiedAt = v.UpdatedAt,
                ModifiedBy = v.UpdatedByUser != null
                    ? $"{v.UpdatedByUser.FirstName} {v.UpdatedByUser.LastName}"
                    : null,
                ApplicationsCount = v.Applications.Count
            })
            .ToListAsync();

        return (vacancies, totalCount);
    }

    public async Task<VacancyResponse?> GetVacancyByIdAsync(Guid id)
    {
        var vacancy = await _context.Vacancies
            .Include(v => v.Applications)
            .Include(v => v.CompetencyMatrixTemplates)
                .ThenInclude(m => m.Competencies)
            .Include(v => v.CreatedByUser)
                .ThenInclude(u => u.Role)
            .Include(v => v.UpdatedByUser)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            return null;

        var competencyMatrixTemplate = vacancy.CompetencyMatrixTemplates
            .FirstOrDefault();

        CompetencyMatrixResponse? competencyMatrixResponse = null;

        if (competencyMatrixTemplate != null)
        {
            var competencyResponses = competencyMatrixTemplate.Competencies
                .Select(c => new CompetencyResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            competencyMatrixResponse = new CompetencyMatrixResponse
            {
                Id = competencyMatrixTemplate.Id,
                VacancyId = id,
                Name = competencyMatrixTemplate.Name,
                VacancyTitle = vacancy.Title,
                Competencies = competencyResponses
            };
        }

        var createdBy = vacancy.CreatedByUser != null
            ? $"{vacancy.CreatedByUser.FirstName} {vacancy.CreatedByUser.LastName}" +
              (vacancy.CreatedByUser.Role != null ? $" ({vacancy.CreatedByUser.Role.Name})" : "")
            : "Unknown";

        var updatedBy = vacancy.UpdatedByUser != null
            ? $"{vacancy.UpdatedByUser.FirstName} {vacancy.UpdatedByUser.LastName}" +
              (vacancy.UpdatedByUser.Role != null ? $" ({vacancy.UpdatedByUser.Role.Name})" : "")
            : null;

        return new VacancyResponse
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Department = vacancy.Department,
            Description = vacancy.Description,
            SalaryFrom = vacancy.SalaryFrom,
            SalaryTo = vacancy.SalaryTo,
            ExperienceRequired = (ExperienceLevel)vacancy.ExperienceRequired,
            EmploymentType = (EmploymentType)vacancy.EmploymentType,
            IsArchived = vacancy.IsArchived,
            CreatedAt = vacancy.CreatedAt,
            CreatedBy = createdBy,
            ModifiedAt = vacancy.UpdatedAt,
            ModifiedBy = updatedBy,
            ApplicationsCount = vacancy.Applications?.Count ?? 0,
            Matrix = competencyMatrixResponse
        };
    }

    public async Task<VacancyResponse> CreateVacancyAsync(CreateVacancyRequest request)
    {
        var existingVacancy = await _context.Vacancies
            .AnyAsync(v => v.Title == request.Title && !v.IsArchived);

        if (existingVacancy)
            throw new InvalidOperationException($"Вакансия с названием '{request.Title}' уже существует");

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Department = request.Department,
            Description = request.Description,
            SalaryFrom = request.SalaryFrom,
            SalaryTo = request.SalaryTo,
            ExperienceRequired = (short)request.ExperienceRequired,
            EmploymentType = (short)request.EmploymentType,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        await _context.Vacancies.AddAsync(vacancy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана вакансия {VacancyId} с названием {VacancyTitle}",
            vacancy.Id, vacancy.Title);

        return await GetVacancyByIdAsync(vacancy.Id)
               ?? throw new Exception("Не удалось получить созданную вакансию");
    }

    public async Task<VacancyResponse> UpdateVacancyAsync(Guid id, UpdateVacancyRequest request)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            throw new KeyNotFoundException($"Вакансия с ID {id} не найдена");

        // Проверка на дубликат названия (исключая текущую)
        var existingVacancy = await _context.Vacancies
            .AnyAsync(v => v.Title == request.Title && v.Id != id && !v.IsArchived);

        if (existingVacancy)
            throw new InvalidOperationException($"Вакансия с названием '{request.Title}' уже существует");

        vacancy.Title = request.Title;
        vacancy.Department = request.Department;
        vacancy.Description = request.Description;
        vacancy.SalaryFrom = request.SalaryFrom;
        vacancy.SalaryTo = request.SalaryTo;
        vacancy.ExperienceRequired = (short)request.ExperienceRequired;
        vacancy.EmploymentType = (short)request.EmploymentType;
        vacancy.UpdatedByUserId = _currentUser.UserId;
        vacancy.UpdatedAt = DateTime.UtcNow;

        _context.Vacancies.Update(vacancy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлена вакансия {VacancyId}", id);

        return await GetVacancyByIdAsync(id)
            ?? throw new Exception("Не удалось получить обновленную вакансию");
    }

    public async Task<bool> ArchiveVacancyAsync(Guid id)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            return false;

        if (vacancy.IsArchived)
            throw new InvalidOperationException("Вакансия уже архивирована");

        vacancy.IsArchived = true;
        vacancy.ArchivedByUserId = _currentUser.UserId;
        vacancy.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Архивирована вакансия {VacancyId}", id);

        return true;
    }

    public async Task<bool> UnarchiveVacancyAsync(Guid id)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            return false;

        if (!vacancy.IsArchived)
            throw new InvalidOperationException("Вакансия не архивирована");

        vacancy.IsArchived = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Разархивирована вакансия {VacancyId}", id);

        return true;
    }

    public async Task<bool> DeleteVacancyAsync(Guid id)
    {
        var vacancy = await _context.Vacancies
            .Include(v => v.Applications)
            .Include(v => v.CompetencyMatrixTemplates)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            return false;

        // Проверяем, есть ли связанные данные
        if (vacancy.Applications.Any())
            throw new InvalidOperationException("Невозможно удалить вакансию, так как есть связанные заявки");

        if (vacancy.CompetencyMatrixTemplates.Any())
            throw new InvalidOperationException("Невозможно удалить вакансию, так как есть связанные матрицы компетенций");

        _context.Vacancies.Remove(vacancy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалена вакансия {VacancyId}", id);

        return true;
    }

    public async Task<bool> VacancyExistsAsync(Guid id)
    {
        return await _context.Vacancies.AnyAsync(v => v.Id == id);
    }

    public async Task<List<CandidateResponse>> GetCandidatesByVacancyIdAsync(Guid vacancyId)
    {
        // Проверяем существование вакансии
        var vacancyExists = await _context.Vacancies
            .AnyAsync(v => v.Id == vacancyId);

        if (!vacancyExists)
            throw new KeyNotFoundException($"Вакансия с ID {vacancyId} не найдена");

        // Получаем кандидатов, которые подали заявку на эту вакансию
        var candidates = await _context.Candidates
            .Include(c => c.Applications)
            .ThenInclude(a => a.Vacancy)
            .Include(c => c.Educations)
            .Include(c => c.WorkExperiences)
            .Where(c => c.Applications.Any(a => a.VacancyId == vacancyId))
            .Select(c => new CandidateResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                MiddleName = c.MiddleName,
                FullName = c.LastName + " " + c.FirstName + " " + c.MiddleName,
                Phone = c.Phone,
                Email = c.Email,
                City = c.City,
                Skills = c.Skills,
                CreatedAt = c.CreatedAt
            })
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return candidates;
    }
}
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

    public async Task<(List<VacancyListResponse> Items, int TotalCount)> GetVacanciesAsync(GetVacanciesQuery query)
    {
        var vacanciesQuery = _context.Vacancies
            .Include(v => v.Applications)
            .AsQueryable();

        // Фильтрация по поиску
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            vacanciesQuery = vacanciesQuery.Where(v =>
                v.Title.ToLower().Contains(search) ||
                (v.Department != null && v.Department.ToLower().Contains(search)) ||
                (v.Description != null && v.Description.ToLower().Contains(search)));
        }

        // Фильтрация по отделу
        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            vacanciesQuery = vacanciesQuery.Where(v => v.Department == query.Department);
        }

        // Фильтрация по архиву
        if (query.IsArchived.HasValue)
        {
            vacanciesQuery = vacanciesQuery.Where(v => v.IsArchived == query.IsArchived.Value);
        }

        var totalCount = await vacanciesQuery.CountAsync();

        var vacancies = await vacanciesQuery
            .OrderByDescending(v => v.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(v => new VacancyListResponse
            {
                Id = v.Id,
                Title = v.Title,
                Department = v.Department,
                SalaryFrom = v.SalaryFrom,
                SalaryTo = v.SalaryTo,
                ExperienceRequired = (ExperienceLevel)v.ExperienceRequired,
                EmploymentType = (EmploymentType)v.EmploymentType,
                IsArchived = v.IsArchived,
                CreatedAt = v.CreatedAt,
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
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
            return null;

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
            ApplicationsCount = vacancy.Applications.Count,
            Competencies = vacancy.CompetencyMatrixTemplates
                .SelectMany(m => m.Competencies)
                .Where(c => !c.IsArchived)
                .Select(c => c.Name)
                .Distinct()
                .ToList()
        };
    }

    public async Task<VacancyResponse> CreateVacancyAsync(CreateVacancyRequest request)
    {
        // Проверка на дубликат названия
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
}
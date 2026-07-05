using HRBars.Application.DTOs.Competency;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class CompetencyService : ICompetencyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompetencyService> _logger;

    public CompetencyService(AppDbContext context, ILogger<CompetencyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<CompetencyResponse> Items, int TotalCount)> GetCompetenciesAsync(GetCompetenciesQuery query)
    {
        var competenciesQuery = _context.Competencies
            .AsQueryable();

        if (query.TemplateId.HasValue)
        {
            competenciesQuery = competenciesQuery.Where(c => c.TemplateId == query.TemplateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            competenciesQuery = competenciesQuery.Where(c => c.Category == query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            competenciesQuery = competenciesQuery.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)) ||
                (c.Category != null && c.Category.ToLower().Contains(search)));
        }

        if (query.IsArchived.HasValue)
        {
            competenciesQuery = competenciesQuery.Where(c => c.IsArchived == query.IsArchived.Value);
        }

        var totalCount = await competenciesQuery.CountAsync();

        var items = await competenciesQuery
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CompetencyResponse
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                Description = c.Description,
                IsArchived = c.IsArchived,
                TemplateId = c.TemplateId
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CompetencyResponse?> GetCompetencyByIdAsync(Guid id)
    {
        var competency = await _context.Competencies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competency == null)
            return null;

        return new CompetencyResponse
        {
            Id = competency.Id,
            Name = competency.Name,
            Category = competency.Category,
            Description = competency.Description,
            IsArchived = competency.IsArchived,
            TemplateId = competency.TemplateId
        };
    }

    public async Task<CompetencyResponse> CreateCompetencyAsync(Guid templateId, CreateCompetencyRequest request)
    {
        // Проверка существования матрицы
        var templateExists = await _context.CompetencyMatrixTemplates
            .AnyAsync(m => m.Id == templateId && !m.IsArchived);

        if (!templateExists)
            throw new KeyNotFoundException($"Матрица компетенций с ID {templateId} не найдена или архивирована");

        // Проверка на дубликат названия в рамках матрицы
        var existingCompetency = await _context.Competencies
            .AnyAsync(c => c.TemplateId == templateId && c.Name == request.Name && !c.IsArchived);

        if (existingCompetency)
            throw new InvalidOperationException($"Компетенция с названием '{request.Name}' уже существует в этой матрице");

        var competency = new Competency
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            IsArchived = false,
            TemplateId = templateId
        };

        await _context.Competencies.AddAsync(competency);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана компетенция {CompetencyId} с названием {CompetencyName} в матрице {TemplateId}",
            competency.Id, competency.Name, templateId);

        return await GetCompetencyByIdAsync(competency.Id)
            ?? throw new Exception("Не удалось получить созданную компетенцию");
    }

    public async Task<CompetencyResponse> UpdateCompetencyAsync(Guid id, UpdateCompetencyRequest request)
    {
        var competency = await _context.Competencies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competency == null)
            throw new KeyNotFoundException($"Компетенция с ID {id} не найдена");

        // Проверка на дубликат названия в рамках матрицы (исключая текущую)
        var existingCompetency = await _context.Competencies
            .AnyAsync(c => c.TemplateId == competency.TemplateId && c.Name == request.Name && c.Id != id && !c.IsArchived);

        if (existingCompetency)
            throw new InvalidOperationException($"Компетенция с названием '{request.Name}' уже существует в этой матрице");

        competency.Name = request.Name;
        competency.Category = request.Category;
        competency.Description = request.Description;
        competency.IsArchived = request.IsArchived;

        _context.Competencies.Update(competency);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлена компетенция {CompetencyId}", id);

        return await GetCompetencyByIdAsync(id)
            ?? throw new Exception("Не удалось получить обновленную компетенцию");
    }

    public async Task<bool> DeleteCompetencyAsync(Guid id)
    {
        var competency = await _context.Competencies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competency == null)
            return false;

        _context.Competencies.Remove(competency);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалена компетенция {CompetencyId}", id);

        return true;
    }

    public async Task<bool> CompetencyExistsAsync(Guid id)
    {
        return await _context.Competencies.AnyAsync(c => c.Id == id);
    }
}
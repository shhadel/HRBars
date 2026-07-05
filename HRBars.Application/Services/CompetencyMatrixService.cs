using HRBars.Application.DTOs.Competency;
using HRBars.Application.DTOs.CompetencyMatrix;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class CompetencyMatrixService : ICompetencyMatrixService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompetencyMatrixService> _logger;

    public CompetencyMatrixService(AppDbContext context, ILogger<CompetencyMatrixService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<CompetencyMatrixListResponse> Items, int TotalCount)> GetCompetencyMatricesAsync(GetCompetencyMatricesQuery query)
    {
        var matricesQuery = _context.CompetencyMatrixTemplates
            .Include(m => m.Vacancy)
            .Include(m => m.Competencies)
            .AsQueryable();

        if (query.VacancyId.HasValue)
        {
            matricesQuery = matricesQuery.Where(m => m.VacancyId == query.VacancyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            matricesQuery = matricesQuery.Where(m =>
                m.Name.ToLower().Contains(search) ||
                (m.Description != null && m.Description.ToLower().Contains(search)));
        }

        if (query.IsArchived.HasValue)
        {
            matricesQuery = matricesQuery.Where(m => m.IsArchived == query.IsArchived.Value);
        }

        var totalCount = await matricesQuery.CountAsync();

        var matrices = await matricesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Description,
                m.IsArchived,
                VacancyTitle = m.Vacancy != null ? m.Vacancy.Title : null,
                CompetenciesCount = m.Competencies.Count
            })
            .ToListAsync();

        var items = matrices.Select(m => new CompetencyMatrixListResponse
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            IsArchived = m.IsArchived,
            VacancyTitle = m.VacancyTitle,
            CompetenciesCount = m.CompetenciesCount
        }).ToList();

        return (items, totalCount);
    }

    public async Task<CompetencyMatrixResponse?> GetCompetencyMatrixByIdAsync(Guid id)
    {
        var matrix = await _context.CompetencyMatrixTemplates
            .Include(m => m.Vacancy)
            .Include(m => m.Competencies)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matrix == null)
            return null;

        return new CompetencyMatrixResponse
        {
            Id = matrix.Id,
            Name = matrix.Name,
            Description = matrix.Description,
            IsArchived = matrix.IsArchived,
            VacancyId = matrix.VacancyId,
            VacancyTitle = matrix.Vacancy?.Title,
            Competencies = matrix.Competencies
                .Where(c => !c.IsArchived)
                .Select(c => new CompetencyResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Category = c.Category,
                    Description = c.Description,
                    IsArchived = c.IsArchived,
                    TemplateId = c.TemplateId
                })
                .ToList()
        };
    }

    public async Task<CompetencyMatrixResponse> CreateCompetencyMatrixAsync(CreateCompetencyMatrixRequest request)
    {
        // Проверка существования вакансии, если указана
        if (request.VacancyId.HasValue)
        {
            var vacancyExists = await _context.Vacancies.AnyAsync(v => v.Id == request.VacancyId.Value);
            if (!vacancyExists)
                throw new KeyNotFoundException($"Вакансия с ID {request.VacancyId.Value} не найдена");
        }

        // Проверка на дубликат названия
        var existingMatrix = await _context.CompetencyMatrixTemplates
            .AnyAsync(m => m.Name == request.Name && !m.IsArchived);
        if (existingMatrix)
            throw new InvalidOperationException($"Матрица с названием '{request.Name}' уже существует");

        var matrix = new CompetencyMatrixTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            VacancyId = request.VacancyId,
            IsArchived = false
        };

        await _context.CompetencyMatrixTemplates.AddAsync(matrix);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана матрица компетенций {MatrixId} с названием {MatrixName}",
            matrix.Id, matrix.Name);

        return await GetCompetencyMatrixByIdAsync(matrix.Id)
            ?? throw new Exception("Не удалось получить созданную матрицу");
    }

    public async Task<CompetencyMatrixResponse> UpdateCompetencyMatrixAsync(Guid id, UpdateCompetencyMatrixRequest request)
    {
        var matrix = await _context.CompetencyMatrixTemplates
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matrix == null)
            throw new KeyNotFoundException($"Матрица компетенций с ID {id} не найдена");

        // Проверка существования вакансии, если указана
        if (request.VacancyId.HasValue)
        {
            var vacancyExists = await _context.Vacancies.AnyAsync(v => v.Id == request.VacancyId.Value);
            if (!vacancyExists)
                throw new KeyNotFoundException($"Вакансия с ID {request.VacancyId.Value} не найдена");
        }

        // Проверка на дубликат названия (исключая текущую)
        var existingMatrix = await _context.CompetencyMatrixTemplates
            .AnyAsync(m => m.Name == request.Name && m.Id != id && !m.IsArchived);
        if (existingMatrix)
            throw new InvalidOperationException($"Матрица с названием '{request.Name}' уже существует");

        matrix.Name = request.Name;
        matrix.Description = request.Description;
        matrix.VacancyId = request.VacancyId;
        matrix.IsArchived = request.IsArchived;

        _context.CompetencyMatrixTemplates.Update(matrix);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлена матрица компетенций {MatrixId}", id);

        return await GetCompetencyMatrixByIdAsync(id)
            ?? throw new Exception("Не удалось получить обновленную матрицу");
    }

    public async Task<bool> DeleteCompetencyMatrixAsync(Guid id)
    {
        var matrix = await _context.CompetencyMatrixTemplates
            .Include(m => m.Competencies)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matrix == null)
            return false;

        // Удаляем все связанные компетенции
        if (matrix.Competencies.Any())
        {
            _context.Competencies.RemoveRange(matrix.Competencies);
        }

        _context.CompetencyMatrixTemplates.Remove(matrix);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалена матрица компетенций {MatrixId}", id);

        return true;
    }

    public async Task<bool> CompetencyMatrixExistsAsync(Guid id)
    {
        return await _context.CompetencyMatrixTemplates.AnyAsync(m => m.Id == id);
    }
}
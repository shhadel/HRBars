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

    public async Task<List<CompetencyMatrixListResponse>> GetMatrixByIdAsync()
    {
        var matrix = await _context.CompetencyMatrixTemplates.ToListAsync();

        List<CompetencyMatrixListResponse> competencyMatrixListResponses = [];
        foreach (var sd in matrix)
        {
            competencyMatrixListResponses.Add(new CompetencyMatrixListResponse() { Id = sd.Id, Name = sd.Name, VacancyId = sd.VacancyId });
        }

        return competencyMatrixListResponses;
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
            VacancyId = matrix.VacancyId,
            VacancyTitle = matrix.Vacancy?.Title,
            Competencies = matrix.Competencies
                .Select(c => new CompetencyResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList()
        };
    }

    public async Task<CompetencyMatrixResponse> CreateCompetencyMatrixAsync(CreateCompetencyMatrixRequest request)
    {
        var matrix = new CompetencyMatrixTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            VacancyId = request.VacancyId
        };

        await _context.CompetencyMatrixTemplates.AddAsync(matrix);
        await _context.SaveChangesAsync();

        foreach (var competencyResponse in request.CompetencyResponses)
        {
            await _context.Competencies.AddAsync(new Competency()
            { Id = competencyResponse.Id, Name = competencyResponse.Name, TemplateId = matrix.Id, Template = matrix });
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана матрица компетенций {MatrixId} с названием {MatrixName}",
            matrix.Id, matrix.Name);

        return await GetCompetencyMatrixByIdAsync(matrix.Id)
            ?? throw new Exception("Не удалось получить созданную матрицу");
    }
}
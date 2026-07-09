using HRBars.Application.DTOs.Education;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class EducationService : IEducationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EducationService> _logger;

    public EducationService(AppDbContext context, ILogger<EducationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EducationResponse> AddEducationAsync(Guid candidateId, CreateEducationRequest request)
    {
        var candidateExists = await _context.Candidates
            .AnyAsync(c => c.Id == candidateId);

        if (!candidateExists)
            throw new KeyNotFoundException($"Кандидат с ID {candidateId} не найден");

        var education = new Education
        {
            Id = Guid.NewGuid(),
            Institution = request.Institution,
            Faculty = request.Faculty,
            Degree = request.Degree,
            StartYear = request.StartYear,
            EndYear = request.EndYear,
            CandidateId = candidateId
        };

        await _context.Educations.AddAsync(education);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Добавлено образование {EducationId} для кандидата {CandidateId}",
            education.Id, candidateId);

        return new EducationResponse
        {
            Id = education.Id,
            Institution = education.Institution,
            Faculty = education.Faculty,
            Degree = education.Degree,
            StartYear = education.StartYear,
            EndYear = education.EndYear,
            CandidateId = education.CandidateId
        };
    }

    public async Task<EducationResponse> UpdateEducationAsync(Guid id, UpdateEducationRequest request)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == id);

        if (education == null)
            throw new KeyNotFoundException($"Образование с ID {id} не найдено");

        education.Institution = request.Institution;
        education.Faculty = request.Faculty;
        education.Degree = request.Degree;
        education.StartYear = request.StartYear;
        education.EndYear = request.EndYear;

        _context.Educations.Update(education);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлено образование {EducationId}", id);

        return new EducationResponse
        {
            Id = education.Id,
            Institution = education.Institution,
            Faculty = education.Faculty,
            Degree = education.Degree,
            StartYear = education.StartYear,
            EndYear = education.EndYear,
            CandidateId = education.CandidateId
        };
    }

    public async Task<bool> DeleteEducationAsync(Guid id)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == id);

        if (education == null)
            return false;

        _context.Educations.Remove(education);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалено образование {EducationId}", id);

        return true;
    }

    public async Task<EducationResponse?> GetEducationByIdAsync(Guid id)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == id);

        if (education == null)
            return null;

        return new EducationResponse
        {
            Id = education.Id,
            Institution = education.Institution,
            Faculty = education.Faculty,
            Degree = education.Degree,
            StartYear = education.StartYear,
            EndYear = education.EndYear,
            CandidateId = education.CandidateId
        };
    }

    public async Task<List<EducationResponse>> GetEducationsByCandidateIdAsync(Guid candidateId)
    {
        var educations = await _context.Educations
            .Where(e => e.CandidateId == candidateId)
            .OrderByDescending(e => e.StartYear)
            .ToListAsync();

        return educations.Select(e => new EducationResponse
        {
            Id = e.Id,
            Institution = e.Institution,
            Faculty = e.Faculty,
            Degree = e.Degree,
            StartYear = e.StartYear,
            EndYear = e.EndYear,
            CandidateId = e.CandidateId
        }).ToList();
    }

    public async Task<bool> CandidateExistsAsync(Guid candidateId)
    {
        return await _context.Candidates.AnyAsync(c => c.Id == candidateId);
    }

    /// <summary>
    /// Поиск учебных заведений по введённому тексту
    /// </summary>
    public async Task<List<string>> SearchInstitutionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<string>();

        query = query.ToLower().Trim();

        return await _context.Educations
            .Where(e => e.Institution != null && e.Institution.ToLower().Contains(query))
            .Select(e => e.Institution)
            .Distinct()
            .OrderBy(i => i)
            .Take(10) // Ограничиваем количество подсказок
            .ToListAsync();
    }
}
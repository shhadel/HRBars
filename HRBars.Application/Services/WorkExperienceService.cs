using HRBars.Application.DTOs.WorkExperience;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services;

public class WorkExperienceService(AppDbContext context, ILogger<WorkExperienceService> logger)
    : IWorkExperienceService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<WorkExperienceService> _logger = logger;

    public async Task<WorkExperienceResponse> AddWorkExperienceAsync(Guid candidateId, CreateWorkExperienceRequest request)
    {
        var candidateExists = await _context.Candidates
            .AnyAsync(c => c.Id == candidateId);

        if (!candidateExists)
            throw new KeyNotFoundException($"Кандидат с ID {candidateId} не найден");

        if (request.EndDate.HasValue && request.EndDate.Value <= request.StartDate)
            throw new InvalidOperationException("Дата окончания должна быть позже даты начала");

        var workExperience = new WorkExperience
        {
            Id = Guid.NewGuid(),
            Company = request.Company,
            Position = request.Position,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            CandidateId = candidateId
        };

        await _context.WorkExperiences.AddAsync(workExperience);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Добавлен опыт работы {WorkExperienceId} для кандидата {CandidateId}",
            workExperience.Id, candidateId);

        return new WorkExperienceResponse
        {
            Id = workExperience.Id,
            Company = workExperience.Company,
            Position = workExperience.Position,
            StartDate = workExperience.StartDate,
            EndDate = workExperience.EndDate,
            Description = workExperience.Description,
            CandidateId = workExperience.CandidateId
        };
    }

    public async Task<WorkExperienceResponse> UpdateWorkExperienceAsync(Guid id, UpdateWorkExperienceRequest request)
    {
        var workExperience = await _context.WorkExperiences
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workExperience == null)
            throw new KeyNotFoundException($"Опыт работы с ID {id} не найден");

        if (request.EndDate.HasValue && request.EndDate.Value <= request.StartDate)
            throw new InvalidOperationException("Дата окончания должна быть позже даты начала");

        workExperience.Company = request.Company;
        workExperience.Position = request.Position;
        workExperience.StartDate = request.StartDate;
        workExperience.EndDate = request.EndDate;
        workExperience.Description = request.Description;

        _context.WorkExperiences.Update(workExperience);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлен опыт работы {WorkExperienceId}", id);

        return new WorkExperienceResponse
        {
            Id = workExperience.Id,
            Company = workExperience.Company,
            Position = workExperience.Position,
            StartDate = workExperience.StartDate,
            EndDate = workExperience.EndDate,
            Description = workExperience.Description,
            CandidateId = workExperience.CandidateId
        };
    }

    public async Task<bool> DeleteWorkExperienceAsync(Guid id)
    {
        var workExperience = await _context.WorkExperiences
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workExperience == null)
            return false;

        _context.WorkExperiences.Remove(workExperience);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удален опыт работы {WorkExperienceId}", id);

        return true;
    }

    public async Task<WorkExperienceResponse?> GetWorkExperienceByIdAsync(Guid id)
    {
        var workExperience = await _context.WorkExperiences
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workExperience == null)
            return null;

        return new WorkExperienceResponse
        {
            Id = workExperience.Id,
            Company = workExperience.Company,
            Position = workExperience.Position,
            StartDate = workExperience.StartDate,
            EndDate = workExperience.EndDate,
            Description = workExperience.Description,
            CandidateId = workExperience.CandidateId
        };
    }

    public async Task<List<WorkExperienceResponse>> GetWorkExperiencesByCandidateIdAsync(Guid candidateId)
    {
        var workExperiences = await _context.WorkExperiences
            .Where(w => w.CandidateId == candidateId)
            .OrderByDescending(w => w.StartDate)
            .ToListAsync();

        return workExperiences.Select(w => new WorkExperienceResponse
        {
            Id = w.Id,
            Company = w.Company,
            Position = w.Position,
            StartDate = w.StartDate,
            EndDate = w.EndDate,
            Description = w.Description,
            CandidateId = w.CandidateId
        }).ToList();
    }

    public async Task<bool> CandidateExistsAsync(Guid candidateId)
    {
        return await _context.Candidates.AnyAsync(c => c.Id == candidateId);
    }
}
using HRBars.Application.DTOs.Candidate;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRBars.Application.Services;

public class CandidateService : ICandidateService
{
    private readonly AppDbContext _context;

    public CandidateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<CandidateResponse>> GetCandidatesAsync(GetCandidates query)
    {
        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        var candidatesQuery = _context.Candidates
            .AsNoTracking()
            .Where(c => c.ArchivedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";

            candidatesQuery = candidatesQuery.Where(c =>
                EF.Functions.ILike(c.FirstName, search) ||
                EF.Functions.ILike(c.LastName, search) ||
                (c.MiddleName != null && EF.Functions.ILike(c.MiddleName, search)) ||
                EF.Functions.ILike(c.Phone, search) ||
                (c.Email != null && EF.Functions.ILike(c.Email, search)) ||
                (c.Skills != null && EF.Functions.ILike(c.Skills, search)) ||
                (c.DesiredVacancy != null && EF.Functions.ILike(c.DesiredVacancy, $"%{search}%")));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            candidatesQuery = candidatesQuery.Where(c =>
                c.City != null &&
                EF.Functions.ILike(c.City, query.City.Trim()));
        }

        var totalCount = await candidatesQuery.CountAsync();

        var items = await candidatesQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CandidateResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                MiddleName = c.MiddleName,
                FullName = BuildFullName(
                    c.LastName,
                    c.FirstName,
                    c.MiddleName),
                DesiredVacancy = c.DesiredVacancy,
                Phone = c.Phone,
                Email = c.Email,
                City = c.City,
                Skills = c.Skills,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<CandidateResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<CandidateDetails?> GetCandidateByIdAsync(Guid id)
    {
        var candidate = await _context.Candidates
            .AsNoTracking()
            .Include(c => c.Educations)
            .Include(c => c.WorkExperiences)
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.Id == id && c.ArchivedAt == null);

        if (candidate == null)
            return null;

        return new CandidateDetails
        {
            Id = candidate.Id,
            FirstName = candidate.FirstName,
            LastName = candidate.LastName,
            MiddleName = candidate.MiddleName,
            FullName = BuildFullName(
                candidate.LastName,
                candidate.FirstName,
                candidate.MiddleName),
            DesiredVacancy = candidate.DesiredVacancy,
            Phone = candidate.Phone,
            Email = candidate.Email,
            City = candidate.City,
            Skills = candidate.Skills,
            ResumeStorageKey = candidate.ResumeStorageKey,
            CreatedAt = candidate.CreatedAt,
            ApplicationsCount = candidate.Applications.Count,

            Educations = candidate.Educations
                .Select(e => new EducationResponse
                {
                    Id = e.Id,
                    Institution = e.Institution,
                    Faculty = e.Faculty,
                    Degree = e.Degree,
                    StartYear = e.StartYear,
                    EndYear = e.EndYear
                })
                .ToList(),

            WorkExperiences = candidate.WorkExperiences
                .Select(w => new WorkExperienceResponse
                {
                    Id = w.Id,
                    Company = w.Company,
                    Position = w.Position,
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    Description = w.Description
                })
                .ToList()
        };
    }

    public async Task<CandidateResponse> CreateCandidateAsync(CreateCandidate request)
    {
        var phone = request.Phone.Trim();
        var email = request.Email?.Trim();

        await ValidateCandidateUniquenessAsync(phone, email);

        var now = DateTime.UtcNow;

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            DesiredVacancy = request.DesiredVacancy?.Trim(),
            Phone = phone,
            Email = email,
            City = request.City?.Trim(),
            Skills = request.Skills?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Candidates.AddAsync(candidate);
        await _context.SaveChangesAsync();

        return ToCandidateResponse(candidate);
    }

    public async Task<CandidateResponse?> UpdateCandidateAsync(Guid id, UpdateCandidate request)
    {
        var candidate = await GetActiveCandidateAsync(id);

        if (candidate == null)
            return null;

        var phone = request.Phone.Trim();
        var email = request.Email?.Trim();

        await ValidateCandidateUniquenessAsync(
            phone,
            email,
            id);

        candidate.FirstName = request.FirstName.Trim();
        candidate.LastName = request.LastName.Trim();
        candidate.MiddleName = request.MiddleName?.Trim();
        candidate.DesiredVacancy = request.DesiredVacancy?.Trim();
        candidate.Phone = phone;
        candidate.Email = email;
        candidate.City = request.City?.Trim();
        candidate.Skills = request.Skills?.Trim();
        candidate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToCandidateResponse(candidate);
    }

    public async Task<bool> ArchiveCandidateAsync(Guid id)
    {
        var candidate = await GetActiveCandidateAsync(id);

        if (candidate == null)
            return false;

        var now = DateTime.UtcNow;

        candidate.ArchivedAt = now;
        candidate.UpdatedAt = now;

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

    private Task<Candidate?> GetActiveCandidateAsync(Guid id)
    {
        return _context.Candidates
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.ArchivedAt == null);
    }

    private async Task ValidateCandidateUniquenessAsync(
        string phone,
        string? email,
        Guid? excludedCandidateId = null)
    {
        var phoneExists = await _context.Candidates.AnyAsync(c =>
            c.ArchivedAt == null &&
            c.Phone == phone &&
            (!excludedCandidateId.HasValue || c.Id != excludedCandidateId.Value));

        if (phoneExists)
            throw new InvalidOperationException(
                "Кандидат с таким номером телефона уже существует.");

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists = await _context.Candidates.AnyAsync(c =>
                c.ArchivedAt == null &&
                c.Email == email &&
                (!excludedCandidateId.HasValue || c.Id != excludedCandidateId.Value));

            if (emailExists)
                throw new InvalidOperationException(
                    "Кандидат с таким Email уже существует.");
        }
    }

    private static CandidateResponse ToCandidateResponse(Candidate candidate)
    {
        return new CandidateResponse
        {
            Id = candidate.Id,
            FirstName = candidate.FirstName,
            LastName = candidate.LastName,
            MiddleName = candidate.MiddleName,
            FullName = BuildFullName(
                candidate.LastName,
                candidate.FirstName,
                candidate.MiddleName),
            DesiredVacancy = candidate.DesiredVacancy,
            Phone = candidate.Phone,
            Email = candidate.Email,
            City = candidate.City,
            Skills = candidate.Skills,
            CreatedAt = candidate.CreatedAt
        };
    }
}
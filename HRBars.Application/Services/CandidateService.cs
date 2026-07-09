using HRBars.Application.DTOs.Application;
using HRBars.Application.DTOs.Candidate;
using HRBars.Application.DTOs.User;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Domain.Enums;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRBars.Application.Services;

public class CandidateService : ICandidateService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CandidateService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<CandidateResponse>> GetCandidatesAsync(GetCandidates query)
    {
        query.Page = Math.Max(query.Page, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        var candidatesQuery = _context.Candidates
            .AsNoTracking();

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
                (c.DesiredVacancy != null && EF.Functions.ILike(c.DesiredVacancy.Title, $"%{search}%")));
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
                DesiredVacancy = c.DesiredVacancy.Title,
                Phone = c.Phone,
                Email = c.Email,
                City = c.City,
                Skills = c.Skills,
                CreatedAt = c.CreatedAt,
                IsActive = c.ArchivedAt == null
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
            .Include(c => c.DesiredVacancy)
            .FirstOrDefaultAsync(c => c.Id == id);

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
            DesiredVacancy = (candidate.DesiredVacancyId).ToString(),
            Phone = candidate.Phone,
            Email = candidate.Email,
            City = candidate.City,
            Skills = candidate.Skills,
            ResumeStorageKey = candidate.ResumeStorageKey,
            CreatedAt = candidate.CreatedAt,
            ApplicationsCount = candidate.Applications.Count,
            IsActive = candidate.ArchivedAt == null,

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

        var vacancyId = Guid.Parse(request.DesiredVacancyId);

        var vacancy = await _context.Vacancies
            .SingleAsync(q => q.Id == vacancyId);

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            DesiredVacancyId = vacancy.Id,
            DesiredVacancy = vacancy,
            Phone = phone,
            Email = email,
            City = request.City?.Trim(),
            Skills = request.Skills?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = _currentUser.UserId,
        };

        await _context.Candidates.AddAsync(candidate);
        await _context.SaveChangesAsync();

        var application = new Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            Status = 1,
            AppliedAt = now,
            CandidateId = candidate.Id,
            Candidate = candidate,
            VacancyId = vacancyId,
            Vacancy = vacancy,
            CreatedByUserId = _currentUser.UserId
        };

        await _context.Applications.AddAsync(application);
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

        var vacancyId = Guid.Parse(request.DesiredVacancy);

        var vacancy = await _context.Vacancies
            .SingleAsync(q => q.Id == vacancyId);

        candidate.FirstName = request.FirstName.Trim();
        candidate.LastName = request.LastName.Trim();
        candidate.MiddleName = request.MiddleName?.Trim();
        candidate.DesiredVacancyId = vacancy.Id;
        candidate.DesiredVacancy = vacancy;
        candidate.Phone = phone;
        candidate.Email = email;
        candidate.City = request.City?.Trim();
        candidate.Skills = request.Skills?.Trim();
        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedByUserId = _currentUser.UserId;

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
        candidate.ArchivedByUserId = _currentUser.UserId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreCandidateAsync(Guid id)
    {
        var candidate = await GetRestoreCandidateAsync(id);

        if (candidate == null)
            return false;

        var now = DateTime.UtcNow;

        candidate.ArchivedAt = null;
        candidate.UpdatedAt = now;
        candidate.ArchivedByUserId = null;

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

    private Task<Candidate?> GetRestoreCandidateAsync(Guid id)
    {
        return _context.Candidates
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.ArchivedAt != null);
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
            DesiredVacancy = candidate.DesiredVacancy.Title,
            Phone = candidate.Phone,
            Email = candidate.Email,
            City = candidate.City,
            Skills = candidate.Skills,
            CreatedAt = candidate.CreatedAt
        };
    }

    public async Task<List<ApplicationResponse>> GetCandidateApplicationAsync(Guid candidateId)
    {
        var candidateExists = await _context.Candidates
            .AnyAsync(c => c.Id == candidateId);

        if (!candidateExists)
            return new List<ApplicationResponse>();

        var applications = await _context.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy)
            .Include(a => a.CreatedByUser)
            .Include(a => a.StatusHistories)
            .Include(a => a.Interviews)
            .Where(a => a.CandidateId == candidateId)
            .Where(a => a.Status != (short)ApplicationStatus.Rejected &&
                        a.Status != (short)ApplicationStatus.Cancelled &&
                        a.Status != (short)ApplicationStatus.Hired)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        if (applications == null || !applications.Any())
            return new List<ApplicationResponse>();

        return applications.Select(application => new ApplicationResponse
        {
            Id = application.Id,
            Status = (ApplicationStatus)application.Status,
            StatusName = ((ApplicationStatus)application.Status).ToString(),
            AppliedAt = application.AppliedAt,
            ClosedAt = application.UpdatedAt,

            Candidate = new ApplicationCandidateDto
            {
                Id = application.Candidate.Id,
                FirstName = application.Candidate.FirstName,
                LastName = application.Candidate.LastName,
                MiddleName = application.Candidate.MiddleName,
                FullName = BuildFullName(
                    application.Candidate.LastName,
                    application.Candidate.FirstName,
                    application.Candidate.MiddleName),
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
                Email = application.CreatedByUser.Email,
                FullName = $"{application.CreatedByUser.FirstName} {application.CreatedByUser.LastName}"
            },

            StatusHistories = application.StatusHistories
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new ApplicationStatusHistoryDto
                {
                    Id = h.Id,
                    Status = (ApplicationStatus)h.Status,
                    StatusName = ((ApplicationStatus)h.Status).ToString(),
                    Comment = h.Comment,
                    ChangedAt = h.ChangedAt
                })
                .ToList(),

            Interviews = application.Interviews
                .OrderByDescending(i => i.InterviewDate)
                .Select(i => new ApplicationInterviewDto
                {
                    Id = i.Id,
                    InterviewDate = i.InterviewDate,
                    Format = i.Format.ToString(),
                    Status = i.Status.ToString(),
                    DurationMinutes = i.DurationMinutes,
                    Location = i.Location,
                    Plan = i.Plan
                })
                .ToList()
        }).ToList();
    }
}
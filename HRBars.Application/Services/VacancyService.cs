using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Vacancy;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRBars.Application.Services
{
    public class VacancyService : IVacancyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VacancyService> _logger;

        public VacancyService(AppDbContext context, ILogger<VacancyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResult<VacancyResponse>> GetVacanciesAsync(GetVacancies query)
        {
            var vacanciesQuery = _context.Vacancies
                .Include(v => v.Applications)
                .Include(v => v.CompetencyMatrixTemplates)
                    .ThenInclude(t => t.Competencies)
                .AsQueryable();

            // Фильтрация
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

            // Архивные показываем только если явно запросили
            if (!query.IncludeArchived)
            {
                vacanciesQuery = vacanciesQuery.Where(v => !v.IsArchived);
            }
            else if (query.IsArchived.HasValue)
            {
                vacanciesQuery = vacanciesQuery.Where(v => v.IsArchived == query.IsArchived.Value);
            }

            var totalCount = await vacanciesQuery.CountAsync();

            var items = await vacanciesQuery
                .OrderByDescending(v => v.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(v => new VacancyResponse
                {
                    Id = v.Id,
                    Title = v.Title,
                    Department = v.Department,
                    Description = v.Description,
                    IsArchived = v.IsArchived,
                    CreatedAt = v.CreatedAt,
                    ApplicationsCount = v.Applications.Count,
                    Competencies = v.CompetencyMatrixTemplates
                        .SelectMany(t => t.Competencies)
                        .Select(c => c.Name)
                        .Distinct()
                        .ToList()
                })
                .ToListAsync();

            return new PaginatedResult<VacancyResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<VacancyDetails?> GetVacancyByIdAsync(Guid id)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Applications)
                    .ThenInclude(a => a.Candidate)
                .Include(v => v.CompetencyMatrixTemplates)
                    .ThenInclude(t => t.Competencies)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vacancy == null)
                return null;

            return new VacancyDetails
            {
                Id = vacancy.Id,
                Title = vacancy.Title,
                Department = vacancy.Department,
                Description = vacancy.Description,
                IsArchived = vacancy.IsArchived,
                CreatedAt = vacancy.CreatedAt,
                ApplicationsCount = vacancy.Applications.Count,
                Competencies = vacancy.CompetencyMatrixTemplates
                    .SelectMany(t => t.Competencies)
                    .Select(c => c.Name)
                    .Distinct()
                    .ToList(),
                RecentApplications = vacancy.Applications
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(10)
                    .Select(a => new ApplicationBrief
                    {
                        Id = a.Id,
                        CandidateName = $"{a.Candidate.LastName} {a.Candidate.FirstName}",
                        Status = GetStatusName(a.Status),
                        AppliedAt = a.AppliedAt
                    })
                    .ToList(),
                CompetencyDetails = vacancy.CompetencyMatrixTemplates
                    .SelectMany(t => t.Competencies)
                    .Select(c => new CompetencyDetails
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Category = c.Category,
                        Description = c.Description,
                        IsArchived = c.IsArchived,
                        Weight = 1, // Можно добавить поле в шаблон
                        MaxScore = 10
                    })
                    .ToList()
            };
        }

        public async Task<VacancyResponse> CreateVacancyAsync(CreateVacancy request, Guid userId)
        {
            // Проверка на дубликат
            var existingVacancy = await _context.Vacancies
                .FirstOrDefaultAsync(v => v.Title == request.Title && !v.IsArchived);

            if (existingVacancy != null)
                throw new InvalidOperationException($"Вакансия с названием '{request.Title}' уже существует");

            // Создаём вакансию
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Department = request.Department,
                Description = request.Description,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Vacancies.AddAsync(vacancy);

            // Если есть шаблон компетенций - привязываем
            if (request.CompetencyTemplateId.HasValue)
            {
                var template = await _context.CompetencyMatrixTemplates
                    .FindAsync(request.CompetencyTemplateId.Value);

                if (template != null && !template.IsArchived)
                {
                    // Создаём копию шаблона для этой вакансии
                    var newTemplate = new CompetencyMatrixTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = template.Name,
                        Description = template.Description,
                        VacancyId = vacancy.Id,
                        IsArchived = false
                    };

                    _context.CompetencyMatrixTemplates.Add(newTemplate);

                    // Копируем компетенции
                    var competencies = await _context.Competencies
                        .Where(c => c.TemplateId == template.Id && !c.IsArchived)
                        .ToListAsync();

                    foreach (var comp in competencies)
                    {
                        _context.Competencies.Add(new Competency
                        {
                            Id = Guid.NewGuid(),
                            Name = comp.Name,
                            Category = comp.Category,
                            Description = comp.Description,
                            TemplateId = newTemplate.Id,
                            IsArchived = false
                        });
                    }
                }
            }

            // Если указаны компетенции вручную
            if (request.CompetencyNames != null && request.CompetencyNames.Any())
            {
                var template = new CompetencyMatrixTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = $"Шаблон для {request.Title}",
                    VacancyId = vacancy.Id,
                    IsArchived = false
                };

                _context.CompetencyMatrixTemplates.Add(template);

                foreach (var compName in request.CompetencyNames.Distinct())
                {
                    _context.Competencies.Add(new Competency
                    {
                        Id = Guid.NewGuid(),
                        Name = compName,
                        TemplateId = template.Id,
                        IsArchived = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await GetVacancyResponseDtoAsync(vacancy.Id);
        }

        public async Task<VacancyResponse?> UpdateVacancyAsync(Guid id, UpdateVacancy request, Guid userId)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                return null;

            // Проверка на дубликат названия
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var duplicate = await _context.Vacancies
                    .FirstOrDefaultAsync(v => v.Title == request.Title && v.Id != id && !v.IsArchived);

                if (duplicate != null)
                    throw new InvalidOperationException($"Вакансия с названием '{request.Title}' уже существует");

                vacancy.Title = request.Title;
            }

            if (request.Department != null)
                vacancy.Department = request.Department;

            if (request.Description != null)
                vacancy.Description = request.Description;

            await _context.SaveChangesAsync();

            return await GetVacancyResponseDtoAsync(id);
        }

        public async Task ArchiveVacancyAsync(Guid id, Guid userId)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                throw new InvalidOperationException("Вакансия не найдена");

            if (vacancy.IsArchived)
                throw new InvalidOperationException("Вакансия уже в архиве");

            vacancy.IsArchived = true;
            await _context.SaveChangesAsync();
        }

        public async Task UnarchiveVacancyAsync(Guid id, Guid userId)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                throw new InvalidOperationException("Вакансия не найдена");

            if (!vacancy.IsArchived)
                throw new InvalidOperationException("Вакансия не в архиве");

            vacancy.IsArchived = false;
            await _context.SaveChangesAsync();
        }

        public async Task<List<CompetencyResponse>> GetVacancyCompetenciesAsync(Guid vacancyId)
        {
            return await _context.Competencies
                .Where(c => c.Template.VacancyId == vacancyId && !c.IsArchived)
                .Select(c => new CompetencyResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Category = c.Category,
                    Description = c.Description,
                    IsArchived = c.IsArchived
                })
                .ToListAsync();
        }

        public async Task<CompetencyResponse?> AddCompetencyToVacancyAsync(
            Guid vacancyId,
            AddCompetencyToVacancy request,
            Guid userId)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.CompetencyMatrixTemplates)
                .FirstOrDefaultAsync(v => v.Id == vacancyId);

            if (vacancy == null)
                return null;

            // Проверка на дубликат
            var existing = await _context.Competencies
                .AnyAsync(c => c.Template.VacancyId == vacancyId && c.Name == request.Name && !c.IsArchived);

            if (existing)
                throw new InvalidOperationException($"Компетенция '{request.Name}' уже добавлена к этой вакансии");

            // Находим или создаём шаблон для вакансии
            var template = vacancy.CompetencyMatrixTemplates.FirstOrDefault(t => !t.IsArchived);
            if (template == null)
            {
                template = new CompetencyMatrixTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = $"Шаблон для {vacancy.Title}",
                    VacancyId = vacancyId,
                    IsArchived = false
                };
                _context.CompetencyMatrixTemplates.Add(template);
                await _context.SaveChangesAsync();
            }

            var competency = new Competency
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                TemplateId = template.Id,
                IsArchived = false
            };

            _context.Competencies.Add(competency);
            await _context.SaveChangesAsync();

            return new CompetencyResponse
            {
                Id = competency.Id,
                Name = competency.Name,
                Category = competency.Category,
                Description = competency.Description,
                IsArchived = competency.IsArchived
            };
        }

        public async Task<bool> RemoveCompetencyFromVacancyAsync(Guid vacancyId, Guid competencyId, Guid userId)
        {
            var competency = await _context.Competencies
                .FirstOrDefaultAsync(c => c.Id == competencyId && c.Template.VacancyId == vacancyId);

            if (competency == null)
                return false;

            // Мягкое удаление (архивация)
            competency.IsArchived = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VacancyExistsAsync(Guid id)
        {
            return await _context.Vacancies.AnyAsync(v => v.Id == id);
        }

        public async Task<bool> IsVacancyArchivedAsync(Guid id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            return vacancy?.IsArchived ?? true;
        }

        private async Task<VacancyResponse> GetVacancyResponseDtoAsync(Guid vacancyId)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Applications)
                .Include(v => v.CompetencyMatrixTemplates)
                    .ThenInclude(t => t.Competencies)
                .FirstOrDefaultAsync(v => v.Id == vacancyId);

            return new VacancyResponse
            {
                Id = vacancy.Id,
                Title = vacancy.Title,
                Department = vacancy.Department,
                Description = vacancy.Description,
                IsArchived = vacancy.IsArchived,
                CreatedAt = vacancy.CreatedAt,
                ApplicationsCount = vacancy.Applications.Count,
                Competencies = vacancy.CompetencyMatrixTemplates
                    .SelectMany(t => t.Competencies)
                    .Select(c => c.Name)
                    .Distinct()
                    .ToList()
            };
        }

        private string GetStatusName(short status)
        {
            return status switch
            {
                0 => "New",
                1 => "Viewed",
                2 => "Interview_Scheduled",
                3 => "Interview_Done",
                4 => "Approved",
                5 => "Rejected",
                6 => "Hired",
                _ => "Unknown"
            };
        }
    }
}

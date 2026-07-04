using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Vacancy;
using System;
using System.Collections.Generic;

namespace HRBars.Application.Interfaces
{
    public interface IVacancyService
    {
        Task<PaginatedResult<VacancyResponse>> GetVacanciesAsync(GetVacancies query);
        Task<VacancyDetails?> GetVacancyByIdAsync(Guid id);
        Task<VacancyResponse> CreateVacancyAsync(CreateVacancy request, Guid userId);
        Task<VacancyResponse?> UpdateVacancyAsync(Guid id, UpdateVacancy request, Guid userId);
        Task ArchiveVacancyAsync(Guid id, Guid userId);
        Task UnarchiveVacancyAsync(Guid id, Guid userId);
        Task<List<CompetencyResponse>> GetVacancyCompetenciesAsync(Guid vacancyId);
        Task<CompetencyResponse?> AddCompetencyToVacancyAsync(Guid vacancyId, AddCompetencyToVacancy request, Guid userId);
        Task<bool> RemoveCompetencyFromVacancyAsync(Guid vacancyId, Guid competencyId, Guid userId);
        Task<bool> VacancyExistsAsync(Guid id);
        Task<bool> IsVacancyArchivedAsync(Guid id);
    }
}

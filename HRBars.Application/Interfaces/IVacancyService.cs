using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Vacancy;
using System;
using System.Collections.Generic;

namespace HRBars.Application.Interfaces
{
    public interface IVacancyService
    {
        Task<(List<VacancyListResponse> Items, int TotalCount)> GetVacanciesAsync(GetVacanciesQuery query);
        Task<VacancyResponse?> GetVacancyByIdAsync(Guid id);
        Task<VacancyResponse> CreateVacancyAsync(CreateVacancyRequest request);
        Task<VacancyResponse> UpdateVacancyAsync(Guid id, UpdateVacancyRequest request);
        Task<bool> ArchiveVacancyAsync(Guid id);
        Task<bool> UnarchiveVacancyAsync(Guid id);
        Task<bool> DeleteVacancyAsync(Guid id);
        Task<bool> VacancyExistsAsync(Guid id);
    }
}
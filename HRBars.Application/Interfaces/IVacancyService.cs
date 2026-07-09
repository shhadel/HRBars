using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Vacancy;
using System;
using System.Collections.Generic;
using HRBars.Application.DTOs.Candidate;

namespace HRBars.Application.Interfaces
{
    public interface IVacancyService
    {
        Task<(List<VacancyResponse> Items, int TotalCount)> GetVacanciesAsync(GetVacanciesQuery query);
        Task<VacancyResponse?> GetVacancyByIdAsync(Guid id);
        Task<VacancyResponse> CreateVacancyAsync(CreateVacancyRequest request);
        Task<VacancyResponse> UpdateVacancyAsync(Guid id, UpdateVacancyRequest request);
        Task<bool> ArchiveVacancyAsync(Guid id);
        Task<bool> UnarchiveVacancyAsync(Guid id);
        Task<List<CandidateResponse>> GetCandidatesByVacancyIdAsync(Guid vacancyId);
    }
}
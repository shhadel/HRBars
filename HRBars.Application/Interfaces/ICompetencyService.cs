using HRBars.Application.DTOs.Competency;

namespace HRBars.Application.Interfaces;

public interface ICompetencyService
{
    Task<(List<CompetencyResponse> Items, int TotalCount)> GetCompetenciesAsync(GetCompetenciesQuery query);
    Task<CompetencyResponse?> GetCompetencyByIdAsync(Guid id);
    Task<CompetencyResponse> CreateCompetencyAsync(Guid templateId, CreateCompetencyRequest request);
    Task<CompetencyResponse> UpdateCompetencyAsync(Guid id, UpdateCompetencyRequest request);
    Task<bool> DeleteCompetencyAsync(Guid id);
    Task<bool> CompetencyExistsAsync(Guid id);
}
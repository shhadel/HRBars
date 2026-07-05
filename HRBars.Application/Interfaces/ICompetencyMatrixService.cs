using HRBars.Application.DTOs.CompetencyMatrix;

namespace HRBars.Application.Interfaces;

public interface ICompetencyMatrixService
{
    Task<(List<CompetencyMatrixListResponse> Items, int TotalCount)> GetCompetencyMatricesAsync(GetCompetencyMatricesQuery query);
    Task<CompetencyMatrixResponse?> GetCompetencyMatrixByIdAsync(Guid id);
    Task<CompetencyMatrixResponse> CreateCompetencyMatrixAsync(CreateCompetencyMatrixRequest request);
    Task<CompetencyMatrixResponse> UpdateCompetencyMatrixAsync(Guid id, UpdateCompetencyMatrixRequest request);
    Task<bool> DeleteCompetencyMatrixAsync(Guid id);
    Task<bool> CompetencyMatrixExistsAsync(Guid id);
}
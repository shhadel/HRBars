using HRBars.Application.DTOs.CompetencyMatrix;

namespace HRBars.Application.Interfaces;

public interface ICompetencyMatrixService
{
    Task<List<CompetencyMatrixListResponse>> GetMatrixByIdAsync();
    Task<CompetencyMatrixResponse?> GetCompetencyMatrixByIdAsync(Guid id);
    Task<CompetencyMatrixResponse> CreateCompetencyMatrixAsync(CreateCompetencyMatrixRequest request);
}
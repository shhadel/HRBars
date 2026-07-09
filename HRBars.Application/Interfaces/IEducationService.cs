using HRBars.Application.DTOs.Education;

namespace HRBars.Application.Interfaces;

public interface IEducationService
{
    Task<EducationResponse> AddEducationAsync(Guid candidateId, CreateEducationRequest request);
    Task<EducationResponse> UpdateEducationAsync(Guid id, UpdateEducationRequest request);
    Task<bool> DeleteEducationAsync(Guid id);
    Task<EducationResponse?> GetEducationByIdAsync(Guid id);
    Task<List<EducationResponse>> GetEducationsByCandidateIdAsync(Guid candidateId);
    Task<bool> CandidateExistsAsync(Guid candidateId);
    Task<List<string>> SearchInstitutionsAsync(string query);
}
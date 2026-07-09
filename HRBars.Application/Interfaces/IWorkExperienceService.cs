using HRBars.Application.DTOs.WorkExperience;

namespace HRBars.Application.Interfaces;

public interface IWorkExperienceService
{
    Task<WorkExperienceResponse> AddWorkExperienceAsync(Guid candidateId, CreateWorkExperienceRequest request);
    Task<WorkExperienceResponse> UpdateWorkExperienceAsync(Guid id, UpdateWorkExperienceRequest request);
    Task<bool> DeleteWorkExperienceAsync(Guid id);
    Task<WorkExperienceResponse?> GetWorkExperienceByIdAsync(Guid id);
    Task<List<WorkExperienceResponse>> GetWorkExperiencesByCandidateIdAsync(Guid candidateId);
    Task<bool> CandidateExistsAsync(Guid candidateId);
    Task<List<string>> SearchCompaniesAsync(string query);
}
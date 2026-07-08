using HRBars.Application.DTOs.Application;

namespace HRBars.Application.Interfaces;

public interface IApplicationService
{
    Task<(List<ApplicationListResponse> Items, int TotalCount)> GetApplicationsAsync(GetApplicationsQuery query);
    Task<ApplicationResponse?> GetApplicationByIdAsync(Guid id);
    Task<ApplicationResponse> CreateApplicationAsync(CreateApplicationRequest request, Guid createdByUserId);
    Task<ApplicationResponse> UpdateApplicationAsync(Guid id, UpdateApplicationRequest request);
    Task<ApplicationResponse> ChangeStatusAsync(Guid id, ChangeStatusRequest request);
    Task<bool> ArchiveApplicationAsync(Guid id);
    Task<bool> ApplicationExistsAsync(Guid id);
    Task<bool> CandidateExistsAsync(Guid candidateId);
    Task<bool> VacancyExistsAsync(Guid vacancyId);
}
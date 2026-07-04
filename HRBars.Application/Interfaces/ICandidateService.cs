using HRBars.Application.DTOs.Candidate;
using HRBars.Application.DTOs.User;

namespace HRBars.Application.Interfaces;

public interface ICandidateService
{
    Task<PaginatedResult<CandidateResponse>> GetCandidatesAsync(GetCandidates query);

    Task<CandidateDetails?> GetCandidateByIdAsync(Guid id);

    Task<CandidateResponse> CreateCandidateAsync(CreateCandidate request);

    Task<CandidateResponse?> UpdateCandidateAsync(Guid id, UpdateCandidate request);

    Task<bool> ArchiveCandidateAsync(Guid id);
}
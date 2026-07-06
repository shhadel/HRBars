using HRBars.Application.DTOs.User;
using HRBars.Application.DTOs.Interview;

namespace HRBars.Application.Interfaces;

public interface IInterviewService
{
    Task<PaginatedResult<InterviewResponse>> GetInterviewsAsync(GetInterviews query);

    Task<InterviewDetails?> GetInterviewByIdAsync(Guid id);

    Task<InterviewResponse> CreateInterviewAsync(CreateInterview request);

    Task<InterviewResponse?> UpdateInterviewAsync(Guid id, UpdateInterview request);

    Task<InterviewResponse?> MakeDecisionAsync(Guid id, MakeDecision request);

    Task<bool> ArchiveInterviewAsync(Guid id);
}
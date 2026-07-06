using HRBars.Application.DTOs.InterviewScore;

namespace HRBars.Application.Interfaces;

public interface IInterviewCompetencyScoreService
{
    Task<List<InterviewCompetencyScoreResponse>> GetScoresAsync(Guid interviewId);

    Task<List<InterviewCompetencyScoreResponse>> SaveScoresAsync(
        Guid interviewId,
        SaveInterviewScoresRequest request);
}